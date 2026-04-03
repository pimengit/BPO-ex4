using System.Collections.Generic;
using System.Linq;
using System.Windows;
using BPO_ex4.StationLogic;

namespace BPO_ex4
{
    public class GlobalStationLinker
    {
        // Храним все загруженные станции по их именам
        private Dictionary<string, StationInstance> _stations = new Dictionary<string, StationInstance>();

        public void AddStation(StationInstance station)
        {
            _stations[station.StationName] = station;
        }

        public void ClearStations()
        {
            _stations.Clear();
        }

        /// <summary>
        /// Соединяет порт одной станции с портом другой.
        /// Например: Порт 1 Северной (UV_EX1) подключен к Порту 3 Южной (UV_EX3).
        /// </summary>
        /// 


        public void ConnectPorts(string sourceStName, int sourcePort, string targetStName, int targetPort)
        {
            if (!_stations.TryGetValue(sourceStName, out var stA) ||
                !_stations.TryGetValue(targetStName, out var stB))
            {
                AppLogger.Log($"[ЛИНКЕР] Ошибка: одна из станций не найдена ({sourceStName} или {targetStName})");
                return;
            }

            // Формируем наши префиксы, которые мы сделали в парсере
            string outPrefix = $"OUT_UV_EX{sourcePort}["; // Что отдает Станция А
            string inPrefix = $"IN_UV_EX{targetPort}[";   // Что принимает Станция Б

            // Ищем все выходные переменные на заданном порту Станции А
            var exportNodes = stA.Ctx.GetAllNodes()
                                   .Where(n => n.Id.StartsWith(outPrefix))
                                   .ToList();

            int linkedCount = 0;
            foreach (var sourceOutNode in exportNodes)
            {
                // Вытаскиваем индекс (например, из "OUT_UV_EX1[5]" достаем "5]")
                string indexPart = sourceOutNode.Id.Substring(outPrefix.Length);

                // Формируем имя целевого входа: "IN_UV_EX3[5]"
                string targetInId = inPrefix + indexPart;

                // Находим или создаем узел-приемник на Станции Б
                Node targetInNode = stB.Ctx.Get(targetInId);

                // === МАГИЯ УВЯЗКИ (Кросс-подписка) ===
                sourceOutNode.Changed += (node) =>
                {
                    // Закидываем сигнал в соседнюю станцию асинхронно
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // Если у тебя ядро принимает значения через Ctx.Set, то:
                        // stB.Ctx.Set(targetInNode.Id, node.Value);

                        // Либо через Engine (как было):
                        stB.Engine.InjectChange(targetInNode, node.Value);
                    });
                };

                // Синхронизируем начальное состояние при запуске
                stB.Engine.InjectChange(targetInNode, sourceOutNode.Value);
                linkedCount++;
            }

            AppLogger.Log($"[ЛИНКЕР] Соединено: {sourceStName} (Порт {sourcePort}) ---> {targetStName} (Порт {targetPort}). Переменных: {linkedCount}");
        }
    }
}