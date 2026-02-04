using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BPO_ex4.StationLogic; // Проверьте namespace

namespace BPO_ex4.Excel
{
    public static class ExcelUpdater
    {
        // 1. ОБНОВЛЕНИЕ СУЩЕСТВУЮЩЕЙ ЯЧЕЙКИ
        public static void UpdateCell(string filePath, string sheetName, int objectIndex, int groupIndex, int inputIndex, Node newSourceNode)
        {
            using var pkg = new ExcelPackage(new FileInfo(filePath));
            var ws = pkg.Workbook.Worksheets[sheetName];
            if (ws == null) throw new Exception($"Sheet {sheetName} not found");

            int targetRow = FindRowByIndex(ws, objectIndex);
            var colIndices = GetGroupColumns(ws, groupIndex);

            if (inputIndex >= colIndices.Count)
                throw new Exception($"Group {groupIndex} has only {colIndices.Count} inputs. Use AddInputToGroup instead.");

            int targetColIst = colIndices[inputIndex];

            // Получаем код для записи
            (string istCode, int? numVal) = ReverseSourceRules.Resolve(sheetName, newSourceNode);

            // Пишем
            ws.Cells[targetRow, targetColIst].Value = istCode;
            ws.Cells[targetRow, targetColIst - 1].Value = numVal; // Колонка №

            pkg.Save();
        }

        // 2. ДОБАВЛЕНИЕ НОВОГО ВХОДА (Вставка колонок)
        public static void AddInputToGroup(string filePath, string sheetName, int objectIndex, int groupIndex, Node newSourceNode)
        {
            using var pkg = new ExcelPackage(new FileInfo(filePath));
            var ws = pkg.Workbook.Worksheets[sheetName];

            // Находим строку объекта
            int targetRow = FindRowByIndex(ws, objectIndex);

            // Находим границы текущей группы
            var colIndices = GetGroupColumns(ws, groupIndex);

            // Нам нужно вставить ПОСЛЕ последнего входа этой группы
            // Последняя колонка "Ист" текущей группы
            int lastColInGroup = colIndices.Last();
            int insertAtCol = lastColInGroup + 1; // Вставляем сразу за ней

            // Вставляем 2 колонки (№ и Ист)
            ws.InsertColumn(insertAtCol, 2);

            // ОБНОВЛЯЕМ ШАПКУ (МАСКУ)
            // Новые колонки должны принадлежать этой же группе, значит маска должна быть "0" (продолжение)
            // insertAtCol - это будет колонка "№", insertAtCol+1 - это "Ист"
            // Маска пишется над "Ист" (четные/нечетные надо проверить по файлу, обычно маска над Ист)
            // В твоем парсере маска читается из Row 1.

            // Если мы вставили колонки, старая маска сдвинулась вправо.
            // Нам нужно в insertAtCol + 1 (новая колонка Ист) записать "0".
            ws.Cells[1, insertAtCol + 1].Value = "0";

            // ЗАПИСЫВАЕМ ДАННЫЕ
            (string istCode, int? numVal) = ReverseSourceRules.Resolve(sheetName, newSourceNode);

            ws.Cells[targetRow, insertAtCol + 1].Value = istCode; // Ист
            ws.Cells[targetRow, insertAtCol].Value = numVal;      // №

            pkg.Save();
        }

        // 3. СОЗДАНИЕ НОВОЙ ПЕРЕМЕННОЙ (Вставка строки)
        public static void CreateNewObject(string filePath, string sheetName, int newIndex, string description)
        {
            using var pkg = new ExcelPackage(new FileInfo(filePath));
            var ws = pkg.Workbook.Worksheets[sheetName];

            // Ищем последнюю заполненную строку (где есть индекс)
            int lastRow = 5;
            while (ws.Cells[lastRow + 1, 2].Value != null)
            {
                lastRow++;
            }

            int newRow = lastRow + 1;

            // Пишем индекс (Колонка B / 2)
            ws.Cells[newRow, 2].Value = newIndex;

            // Пишем описание (Колонка C / 3)
            ws.Cells[newRow, 3].Value = description;

            // Можно скопировать форматирование с предыдущей строки, если нужно
            // ws.Cells[lastRow, 1, lastRow, ws.Dimension.End.Column].Copy(ws.Cells[newRow, 1]);

            pkg.Save();
        }

        // --- ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ---

        private static int FindRowByIndex(ExcelWorksheet ws, int index)
        {
            int startRow = 5;
            for (int r = startRow; r <= ws.Dimension.End.Row; r++)
            {
                var val = ws.Cells[r, 2].Value?.ToString();
                if (val == index.ToString()) return r;
            }
            throw new Exception($"Object index {index} not found.");
        }

        private static List<int> GetGroupColumns(ExcelWorksheet ws, int targetGroupIndex)
        {
            var indices = new List<int>();
            int col = 5;
            int currentGroupIdx = 0;

            while (true)
            {
                var maskVal = ws.Cells[1, col].Value?.ToString();
                if (string.IsNullOrWhiteSpace(maskVal)) break;

                if (maskVal == "1") currentGroupIdx++;

                if (currentGroupIdx == targetGroupIndex)
                {
                    indices.Add(col); // Первая колонка группы ("Ист")

                    // Ищем продолжение ("0")
                    int nextCol = col + 2;
                    while (ws.Cells[1, nextCol].Value?.ToString() == "0")
                    {
                        indices.Add(nextCol);
                        nextCol += 2;
                    }
                    return indices;
                }
                col += 2;
            }
            throw new Exception($"Group {targetGroupIndex} not found in mask.");
        }
    }
}