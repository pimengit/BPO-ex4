using OfficeOpenXml;
using System.Linq;
using System.Collections.Generic;
using BPO_ex4.StationLogic;

public static class ExcelUpdater
{
    public static void UpdateCell(string filePath, string sheetName, int objectIndex, int groupIndex, int inputIndex, Node newSourceNode)
    {
        using var pkg = new ExcelPackage(new System.IO.FileInfo(filePath));
        var ws = pkg.Workbook.Worksheets[sheetName];

        // 1. ИЩЕМ СТРОКУ (Row) по индексу объекта
        // В ExcelParser индекс читался из колонки 2 (col = 2), но надо проверить парсер.
        // В парсере: var idxCell = ws.Cells[row, 2].Value;
        int targetRow = -1;
        int startRow = 5;

        for (int r = startRow; r <= ws.Dimension.End.Row; r++)
        {
            var cellVal = ws.Cells[r, 2].Value?.ToString(); // Колонка B
            if (cellVal == objectIndex.ToString())
            {
                targetRow = r;
                break;
            }
        }

        if (targetRow == -1) throw new System.Exception($"Object [{objectIndex}] not found in sheet {sheetName}");

        // 2. ИЩЕМ КОЛОНКУ (Col) по маске заголовка
        // Нам нужно воспроизвести логику "GroupsMapping" из ExcelParser
        var groupsMapping = RebuildGroupsMapping(ws);

        if (groupIndex >= groupsMapping.Count)
            throw new System.Exception($"Group {groupIndex} not found in mask");

        var colIndices = groupsMapping[groupIndex];

        if (inputIndex >= colIndices.Count)
            throw new System.Exception($"Input slot {inputIndex} not found in Group {groupIndex}");

        int targetCol = colIndices[inputIndex]; // Это колонка "Ист" (Source)

        // 3. КОНВЕРТИРУЕМ Node ID -> Excel Code ("2.3", "3.0")
        // Это самое важное: нам нужен "Обратный SourceRules"
        (string istCode, int? numVal) = ReverseSourceRules.Resolve(sheetName, newSourceNode);

        // 4. ЗАПИСЫВАЕМ
        // Колонка Ист
        ws.Cells[targetRow, targetCol].Value = istCode;

        // Колонка № (всегда слева от Ист, см. парсер: numCell = cIndex - 1)
        if (numVal.HasValue)
            ws.Cells[targetRow, targetCol - 1].Value = numVal.Value;
        else
            ws.Cells[targetRow, targetCol - 1].Value = null;

        pkg.Save();
    }

    // Тот же код, что в ExcelParser, только возвращает карту
    private static List<List<int>> RebuildGroupsMapping(ExcelWorksheet ws)
    {
        var map = new List<List<int>>();
        map.Add(new List<int>()); // Нулевая группа пустая

        int col = 5;
        List<int> currentGroup = null;

        while (true)
        {
            var maskVal = ws.Cells[1, col].Value?.ToString();
            if (string.IsNullOrWhiteSpace(maskVal)) break;

            if (maskVal == "1")
            {
                currentGroup = new List<int>();
                currentGroup.Add(col); // Добавляем колонку "Ист"
                map.Add(currentGroup);
            }
            else if (maskVal == "0")
            {
                if (currentGroup != null)
                    currentGroup.Add(col);
            }
            col += 2; // Шаг 2, так как пары (№, Ист)
        }
        return map;
    }
}