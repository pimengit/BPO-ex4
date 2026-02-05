using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using BPO_ex4.StationLogic; // Убедитесь, что Node здесь виден

namespace BPO_ex4.Excel
{
    public class ExcelSession : IDisposable
    {
        private ExcelPackage _package;
        private string _filePath;

        public bool IsLoaded => _package != null;

        public void Load(string path)
        {
            Dispose();
            _filePath = path;
            // Открываем с правами на чтение и запись, но разрешаем другим читать (FileShare.Read)
            var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            _package = new ExcelPackage(stream);
        }

        private ExcelWorksheet GetSheet(string sheetName)
        {
            if (_package == null) throw new Exception("Session not loaded");
            
            // Если имя не передано, берем первый лист
            if (string.IsNullOrEmpty(sheetName)) 
                return _package.Workbook.Worksheets.FirstOrDefault();
            
            var ws = _package.Workbook.Worksheets[sheetName];
            if (ws == null) throw new Exception($"Sheet '{sheetName}' not found");
            return ws;
        }

        // --- МЕТОДЫ ЗАПИСИ (5 аргументов и 4 аргумента) ---

        public void UpdateCellInMemory(string sheetName, int objectIndex, int groupIndex, int inputIndex, Node newSourceNode)
        {
            var ws = GetSheet(sheetName);
            int targetRow = FindRowByIndex(ws, objectIndex);
            var colIndices = GetGroupColumns(ws, groupIndex);

            if (inputIndex >= colIndices.Count) throw new Exception("Input index out of range");

            int targetColIst = colIndices[inputIndex];
            
            (string istCode, int? numVal) = ReverseSourceRules.Resolve(ws.Name, newSourceNode);

            ws.Cells[targetRow, targetColIst].Value = istCode;
            ws.Cells[targetRow, targetColIst - 1].Value = numVal;
        }

        public void AddInputInMemory(string sheetName, int objectIndex, int groupIndex, Node newSourceNode)
        {
            var ws = GetSheet(sheetName);
            int targetRow = FindRowByIndex(ws, objectIndex);
            var colIndices = GetGroupColumns(ws, groupIndex);
            
            int lastColInGroup = colIndices.Last(); 
            int insertAtCol = lastColInGroup + 1;

            // Вставляем 2 колонки
            ws.InsertColumn(insertAtCol, 2);
            // Обновляем маску заголовка (строка 1)
            ws.Cells[1, insertAtCol + 1].Value = "0"; 

            (string istCode, int? numVal) = ReverseSourceRules.Resolve(ws.Name, newSourceNode);
            
            ws.Cells[targetRow, insertAtCol + 1].Value = istCode;
            ws.Cells[targetRow, insertAtCol].Value = numVal;
        }

        public void Save()
        {
            if (_package != null) _package.Save();
        }

        public void Dispose()
        {
            _package?.Dispose();
            _package = null;
        }

        // --- ХЕЛПЕРЫ ПОИСКА ---
        private int FindRowByIndex(ExcelWorksheet ws, int index)
        {
            int startRow = 5;
            int endRow = ws.Dimension?.End.Row ?? 5000;
            for (int r = startRow; r <= endRow; r++)
            {
                // Сравниваем как текст, чтобы избежать ошибок типов
                if (ws.Cells[r, 2].Text == index.ToString()) return r;
            }
            throw new Exception($"Index {index} not found on sheet {ws.Name}");
        }

        private List<int> GetGroupColumns(ExcelWorksheet ws, int targetGroupIndex)
        {
            var indices = new List<int>();
            int col = 5;
            int currentGroupIdx = 0;
            int maxCol = ws.Dimension?.End.Column ?? 500;

            while (col < maxCol)
            {
                var maskVal = ws.Cells[1, col].Text;
                if (string.IsNullOrWhiteSpace(maskVal)) break;

                if (maskVal == "1") currentGroupIdx++;

                if (currentGroupIdx == targetGroupIndex)
                {
                    indices.Add(col);
                    // Проверяем продолжение группы (маска "0")
                    int nextCol = col + 2; 
                    while (nextCol < maxCol && ws.Cells[1, nextCol].Text == "0")
                    {
                        indices.Add(nextCol);
                        nextCol += 2;
                    }
                    return indices;
                }
                col += 2;
            }
            throw new Exception($"Group {targetGroupIndex} not found");
        }
    }
}