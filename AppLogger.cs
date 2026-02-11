using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace BPO_ex4
{
    public static class AppLogger
    {
        // Коллекция сообщений, к которой привяжется окно
        public static ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

        // Лимит сообщений, чтобы память не текла (например, последние 1000)
        private const int MaxLogCount = 1000;

        public static void Log(string message)
        {
            // Используем Dispatcher, так как лог могут вызывать из разных потоков
            System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                Messages.Insert(0, $"[{time}] {message}"); // Добавляем в начало (сверху свежие)

                if (Messages.Count > MaxLogCount)
                {
                    Messages.RemoveAt(Messages.Count - 1);
                }
            });
        }
    }
}