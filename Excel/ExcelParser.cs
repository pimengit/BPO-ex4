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
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);

            // Используем поток, чтобы не блокировать файл
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var pkg = new ExcelPackage(stream);

            foreach (var ws in pkg.Workbook.Worksheets)
            {
                if (ws.Name.Contains("Address")) ParseTrafficLights(ws, ctx);
                if (ws.Name.Contains("_CONF") || ws.Name.Contains("Station_ID")) continue;
                if (ws.Name.Contains("_"))
                {
                    ParseSheet(ws, ctx); 
                }
                
            }
        }



        // Вызови этот метод внутри основного Load, когда найдешь вкладку Adress
        public static void ParseTrafficLights(ExcelWorksheet ws, Context ctx)
        {
            ctx.LightConfigs.Clear();

            if (ws.Dimension == null) return;
            int endRow = ws.Dimension.End.Row;

            // 1. ПОИСК СТАРТА (МГНОВЕННО)
            // Выгружаем ТОЛЬКО колонку B (Тип) в память
            var typeColumn = ws.Cells[1, 2, endRow, 2].Value as object[,];
            int startRow = -1;

            // Бежим по массиву в памяти (это наносекунды)
            for (int i = 0; i < typeColumn.GetLength(0); i++)
            {
                var val = typeColumn[i, 0]?.ToString();
                if (val != null && val.Contains("Светофор", StringComparison.OrdinalIgnoreCase))
                {
                    startRow = i + 1; // Excel индекс = индекс массива + 1
                    break; // Нашли! Выходим.
                }
            }

            // Если светофоров нет вообще — выходим
            if (startRow == -1) return;

            // 2. ЧИТАЕМ ДАННЫЕ С НУЖНОЙ СТРОКИ
            // Берем данные с startRow до конца. Колонки B, C, D (2,3,4)
            var range = ws.Cells[startRow, 2, endRow, 5];
            object[,] values = range.Value as object[,];

            if (values == null) return;

            int rowsCount = values.GetLength(0);
            string currentLightName = null;

            for (int r = 0; r < rowsCount; r++)
            {
                // Индексы в куске: 0=B (Тип), 1=C (Имя), 2=D (Цвет)
                string typeVal = values[r, 0]?.ToString();
                string nameVal = values[r, 1]?.ToString();
                string colorVal = values[r, 3]?.ToString();

                // 3. ЛОГИКА ПАРСИНГА
                // Если есть ИМЯ - это начало нового блока
                if (!string.IsNullOrWhiteSpace(nameVal))
                {
                    currentLightName = null; // Сброс, чтобы не писать в старый

                    // Если тип не пустой и это Светофор -> Запоминаем имя
                    if (!string.IsNullOrWhiteSpace(typeVal) &&
                        typeVal.Contains("Светофор", StringComparison.OrdinalIgnoreCase))
                    {
                        currentLightName = nameVal.Trim();
                        if (!ctx.LightConfigs.ContainsKey(currentLightName))
                        {
                            ctx.LightConfigs[currentLightName] = new List<SignalColor>();
                        }
                    }
                    // Иначе (если это Реле/Датчик) currentLightName останется null
                }

                // Если мы внутри светофора -> пишем цвет
                if (currentLightName != null && !string.IsNullOrWhiteSpace(colorVal))
                {
                    var color = SignalHelpers.Parse(colorVal);
                    if (color != SignalColor.Unknown)
                    {
                        ctx.LightConfigs[currentLightName].Add(color);
                    }
                }
            }
        }
        static void ParseSheet(ExcelWorksheet ws, Context ctx)
        {
            // 1. АНАЛИЗ МАСКИ (Ваша надежная логика без Dimension)
            var groupsMapping = new List<List<int>>();
            groupsMapping.Add(new List<int>()); // 0-я группа пустая

            int col;
            if (ws.Name.Contains("UV_EX"))
                col = 6;
            else
                col = 5;
                List<int> currentGroup = null;

            while (true)
            {
                // Используем .Value, так как .Text может зависеть от ширины колонки или зума
                var maskVal = ws.Cells[1, col].Value?.ToString();

                // Если пусто - значит конец таблицы
                if (string.IsNullOrWhiteSpace(maskVal)) break;

                if (maskVal == "1")
                {
                    currentGroup = new List<int>();
                    currentGroup.Add(col);
                    groupsMapping.Add(currentGroup);
                }
                else if (maskVal == "0")
                {
                    if (currentGroup != null) currentGroup.Add(col);
                }
                col += 2;
            }

            // 2. ЧТЕНИЕ СТРОК
            int row = 5;
            while (true)
            {
                var stopend = ws.Cells[row, 3].Value?.ToString();
                var idxCell = ws.Cells[row, 2].Value;

                // Условия выхода
                if (string.IsNullOrEmpty(stopend)) break;
                if (idxCell == null || string.IsNullOrWhiteSpace(idxCell.ToString())) break;

                if (!int.TryParse(idxCell.ToString(), out int objectIndex))
                {
                    row++; continue;
                }

                // Создаем структуру для групп
                var inputs = new List<Node>[groupsMapping.Count];

                for (int i = 1; i < groupsMapping.Count; i++)
                {
                    var colIndices = groupsMapping[i];
                    var nodeList = new List<Node>();

                    foreach (var cIndex in colIndices)
                    {
                        var istCell = ws.Cells[row, cIndex].Value?.ToString();
                        var numCell = ws.Cells[row, cIndex - 1].Value?.ToString();

                        if (!string.IsNullOrWhiteSpace(istCell))
                        {
                            string ist = istCell.Trim();
                            if (ist.Length > 20 && ist.Contains(" ")) continue;

                            int? num = int.TryParse(numCell, out int n) ? n : (int?)null;

                            try
                            {
                                // Важно: SourceRules должен быть у вас в проекте
                                string srcId = SourceRules.Resolve(ws.Name, ist, num);
                                nodeList.Add(ctx.Get(srcId));
                            }
                            catch { }
                        }
                    }
                    inputs[i] = nodeList;
                }

                // 3. СОЗДАНИЕ
                try
                {
                    var logic = CreateLogic(ws.Name);
                    // Вызываем Фабрику (она пропишет Dependents)
                    VariableFactory.Create(ctx, ws.Name, objectIndex, inputs, logic, stopend);
                }
                catch { }

                row++;
            }
        }

        static SheetLogic CreateLogic(string sheetName)
        {
            var typeName = $"BPO_ex4.LogicSheets.{sheetName}";
            var type = Type.GetType(typeName);

            if (type == null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(typeName);
                    if (type != null) break;
                }
            }

            // Если не нашли логику - возвращаем пустую заглушку, чтобы не падало
            if (type == null) return new SheetLogic();

            return (SheetLogic)Activator.CreateInstance(type);
        }
    }
}