using System;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Excel
{
    public static class ExcelParser
    {
        public static void Load(string path, Context ctx)
        {
            using var pkg = new ExcelPackage(new FileInfo(path));

            foreach (var ws in pkg.Workbook.Worksheets)
            {
                if (ws.Name.Contains("_CONF"))
                    continue;
                Console.WriteLine($"Parsing sheet: {ws.Name}");
                ParseSheet(ws, ctx);
            }
        }

        static void ParseSheet(ExcelWorksheet ws, Context ctx)
        {
            // 1. АНАЛИЗ МАСКИ
            // Мы запоминаем индексы колонок Excel, которые относятся к одной группе
            var groupsMapping = new List<List<int>>();
            int col = 5;

            // Добавляем пустую группу для индекса 0, чтобы нумерация шла с 1 (как в ТТ)
            groupsMapping.Add(new List<int>());

            List<int> currentGroup = null;

            while (true)
            {
                var maskVal = ws.Cells[1, col].Value?.ToString();
                if (string.IsNullOrWhiteSpace(maskVal)) break;

                if (maskVal == "1")
                {
                    // Новая группа
                    currentGroup = new List<int>();
                    currentGroup.Add(col);
                    groupsMapping.Add(currentGroup);
                }
                else if (maskVal == "0")
                {
                    // Продолжение предыдущей группы
                    if (currentGroup != null)
                        currentGroup.Add(col);
                }
                col += 2;
            }

            // 2. ЧТЕНИЕ СТРОК
            int row = 5;
            while (true)
            {
                var stopend = ws.Cells[row, 3].Value;
                var idxCell = ws.Cells[row, 2].Value;

                // Проверяем конец таблицы
                if (stopend == "") break;
                if (idxCell == null) break; // Доп. проверка, чтобы не упасть на пустом индекса

                if (!int.TryParse(idxCell.ToString(), out int objectIndex)) break;

                // Создаем структуру для SheetLogic: массив списков
                var inputs = new List<Node>[groupsMapping.Count];

                // Заполняем группы
                for (int i = 1; i < groupsMapping.Count; i++) // i=1, т.к. 0 пропускаем
                {
                    var colIndices = groupsMapping[i];
                    var nodeList = new List<Node>();

                    foreach (var cIndex in colIndices)
                    {
                        // Твои исправленные индексы:
                        // numCell = cIndex + 1
                        // istCell = cIndex + 2
                        var istCell = ws.Cells[row, cIndex].Value;
                        var numCell = ws.Cells[row, cIndex - 1].Value;

                        if (istCell != null && !string.IsNullOrWhiteSpace(istCell.ToString()))
                        {
                            string ist = istCell.ToString().Trim();

                            // Доп. защита: если текст слишком длинный и содержит пробелы (похоже на коммент), пропускаем
                            if (ist.Length > 15 && ist.Contains(" "))
                                continue;

                            int? num = null;
                            if (numCell != null && int.TryParse(numCell.ToString(), out int n))
                                num = n;

                            // 🛡️ БЛОК ЗАЩИТЫ ОТ ОШИБОК ПАРСИНГА
                            try
                            {
                                string srcId = SourceRules.Resolve(ws.Name, ist, num);
                                nodeList.Add(ctx.Get(srcId));
                            }
                            catch (Exception ex)
                            {
                                // Если попался мусор вместо адреса — пишем желтым в консоль и идем дальше
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"[WARN] {ws.Name} Row {row}: Ignored '{ist}' (Group {i}). Err: {ex.Message}");
                                Console.ResetColor();
                            }
                        }
                    }
                    inputs[i] = nodeList;
                }

                // 3. СОЗДАНИЕ
                var logic = CreateLogic(ws.Name);
                var desc = ws.Cells[row, 3].Value?.ToString();

                VariableFactory.Create(ctx, ws.Name, objectIndex, inputs, logic, desc);

                row++;
            }
        }

        static SheetLogic CreateLogic(string sheetName)
        {
            var typeName = $"BPO_ex4.LogicSheets.{sheetName}";
            var type = Type.GetType(typeName);
            if (type == null)
                throw new Exception($"Logic class not found: {typeName}");

            return (SheetLogic)Activator.CreateInstance(type);
        }
    }
}