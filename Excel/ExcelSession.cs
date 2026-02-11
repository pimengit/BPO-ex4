using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Excel
{
    public class ExcelSession : IDisposable
    {
        private ExcelPackage _package;
        private string _filePath;
        public bool IsLoaded => _package != null;

        public void Load(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
            _filePath = path;

            if (_package != null) _package.Dispose();

            // Открываем поток с правами на чтение и запись
            var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            _package = new ExcelPackage(stream);
        }

        public void Save()
        {
            if (_package != null)
            {
                _package.Save(); // Сохраняем изменения на диск
            }
        }

        public void Dispose()
        {
            _package?.Dispose();
        }

        // === ГЛАВНЫЙ МЕТОД ЗАПИСИ (С АВТО-РАСШИРЕНИЕМ) ===

        public void AddInputInMemory(string sheetName, int objectIndex, int groupIndex, Node newSourceNode)
        {
            var ws = GetSheet(sheetName);
            int row = FindObjectRow(ws, objectIndex);

            // Получаем список колонок (Value), которые уже есть у этой группы
            var cols = GetGroupColumns(ws, groupIndex);

            // 1. Пробуем найти пустую ячейку в существующих колонках
            foreach (var col in cols)
            {
                var cellValue = ws.Cells[row, col].Text;
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    // Место есть -> пишем и выходим
                    WriteNodeToCells(ws, row, col, newSourceNode);
                    return;
                }
            }

            // 2. Если места нет -> РАСШИРЯЕМ ГРУППУ
            if (cols.Count > 0)
            {
                int lastValCol = cols[cols.Count - 1]; // Последняя колонка значений этой группы

                // Нам нужно вставить 2 новых столбца СРАЗУ ПОСЛЕ последней колонки
                // Индекс вставки = lastValCol + 1
                int insertIndex = lastValCol + 1;

                // Вставляем 2 столбца (один под Индекс, второй под Значение)
                ws.InsertColumn(insertIndex, 2);

                // === ВАЖНО: ПРОПИСЫВАЕМ МАСКУ ===
                // Новый столбец значений будет под индексом (insertIndex + 1)
                // Ставим там "0", чтобы пометить его как продолжение группы
                ws.Cells[1, insertIndex + 1].Value = "0";

                // Пишем данные в эти новые колонки
                WriteNodeToCells(ws, row, insertIndex + 1, newSourceNode);
            }
            else
            {
                throw new Exception($"Group {groupIndex} columns not found!");
            }
        }

        public void UpdateCellInMemory(string sheetName, int objectIndex, int groupIndex, int inputIndex, Node newSourceNode)
        {
            var ws = GetSheet(sheetName);
            int row = FindObjectRow(ws, objectIndex);
            var cols = GetGroupColumns(ws, groupIndex);

            if (inputIndex < cols.Count)
            {
                int targetCol = cols[inputIndex];
                WriteNodeToCells(ws, row, targetCol, newSourceNode);
            }
            else
            {
                // Если пытаемся обновить несуществующий индекс, можно тоже вызвать расширение, 
                // но обычно Update вызывается для существующих ячеек.
                throw new Exception($"Input index {inputIndex} out of range for Excel structure.");
            }
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

        private ExcelWorksheet GetSheet(string name)
        {
            var ws = _package.Workbook.Worksheets[name];
            if (ws == null) throw new Exception($"Sheet {name} not found in Excel.");
            return ws;
        }

        private int FindObjectRow(ExcelWorksheet ws, int objIndex)
        {
            int row = 5;
            while (true)
            {
                var cellVal = ws.Cells[row, 2].Value;
                if (cellVal == null) break;

                if (cellVal.ToString() == objIndex.ToString()) return row;

                row++;
                if (row > 10000) break;
            }
            throw new Exception($"Object index {objIndex} not found in sheet {ws.Name}");
        }

        private void WriteNodeToCells(ExcelWorksheet ws, int row, int col, Node node)
        {
            // Разбираем ID вида TYPE[INDEX] на имя и номер
            string name = node.Id;
            string index = "";

            if (node.Id.Contains("["))
            {
                int start = node.Id.IndexOf("[");
                int end = node.Id.IndexOf("]");
                name = node.Id.Substring(0, start);
                index = node.Id.Substring(start + 1, end - start - 1);
            }

            // Пишем: Col = Имя, Col-1 = Номер
            ws.Cells[row, col].Value = name;
            ws.Cells[row, col - 1].Value = index;
        }

        // Надежный поиск колонок (такой же, как мы сделали ранее)
        private List<int> GetGroupColumns(ExcelWorksheet ws, int targetGroupIndex)
        {
            var result = new List<int>();
            int currentGroupCounter = 0;
            int col = 5;

            while (true)
            {
                var maskVal = ws.Cells[1, col].Text;
                if (string.IsNullOrWhiteSpace(maskVal)) break;

                if (maskVal == "1") currentGroupCounter++;

                if (currentGroupCounter == targetGroupIndex)
                {
                    result.Add(col);
                    int subCol = col + 2;
                    while (true)
                    {
                        var subMask = ws.Cells[1, subCol].Text;
                        if (subMask == "0")
                        {
                            result.Add(subCol);
                            subCol += 2;
                        }
                        else return result;
                    }
                }
                col += 2;
                if (col > 2000) break;
            }
            throw new Exception($"Group {targetGroupIndex} not found in header mask.");
        }
    }
}