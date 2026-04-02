using BPO_ex4.StationLogic;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace BPO_ex4.Visuals
{
    public class TopPanelViewModel : INotifyPropertyChanged
    {
        private SimulationEngine _engine;

        // Узлы
        private Node _startNode;
        private Node _kodOdNode;
        private Node _kodPodNode;
        private Node _kodOodNode;
        private Node _kodTodNode;

        // Курки
        private Node _kodPodDk;
        private Node _kodOodDk;

        // Таймер для ОД
        private DispatcherTimer _timer;
        private int _timeLeft;

        // --- СВОЙСТВА ДЛЯ ПРИВЯЗКИ XAML ---

        // 1. Кнопка START
        public Brush StartBrush => (_startNode?.Value == true) ? Brushes.LimeGreen : Brushes.LightGray;
        public ICommand ToggleStartCommand { get; }

        // 2. Кнопка ОД
        public Brush OdBrush => (_kodOdNode?.Value == true) ? Brushes.Yellow : Brushes.LightGray;
        public bool IsOdOodEnabled => _kodOdNode?.Value == true; // Общее свойство доступности

        private string _odText = "ОД";
        public string OdText
        {
            get => _odText;
            set { _odText = value; RaisePropertyChanged(nameof(OdText)); }
        }
        public ICommand ClickOdCommand { get; }

        // 3. Кнопка ООД
        public Brush OodBrush => OdBrush; // Цвет такой же как у ОД
        public ICommand ClickOodCommand { get; }

        public TopPanelViewModel()
        {
            // Настраиваем таймер
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            // Инициализируем команды
            ToggleStartCommand = new SimpleCommand(() =>
            {
                if (_engine != null && _startNode != null)
                {
                    _engine.InjectChange(_startNode, !_startNode.Value);
                }
            });

            ClickOdCommand = new SimpleCommand(() =>
            {
                if (IsOdOodEnabled)
                {
                    PulseNode(_kodPodDk, "KOD_POD_DK");
                    StartTimer(); // Перезапускаем таймер при клике
                }
            });

            ClickOodCommand = new SimpleCommand(() =>
            {
                if (IsOdOodEnabled)
                {
                    PulseNode(_kodOodDk, "KOD_OOD_DK");
                }
            });
        }

        public void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // Ищем узлы
            _startNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == "START");
            _kodOdNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == "KOD_OD[1]");
            _kodPodNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == "KOD_POD[1]");
            _kodOodNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == "KOD_OOD[1]");
            _kodTodNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == "KOD_tOD[1]");

            // Ищем курки внутри POD и OOD
            _kodPodDk = FindMkNode(ctx, _kodPodNode, "KOD_POD_DK");
            _kodOodDk = FindMkNode(ctx, _kodOodNode, "KOD_OOD_DK");

            // Подписываемся на изменения
            if (_startNode != null) _startNode.Changed += _ => OnLogicChanged();
            if (_kodOdNode != null) _kodOdNode.Changed += _ => OnLogicChanged();

            OnLogicChanged();
        }

        private void OnLogicChanged()
        {
            // Если включился KOD_OD[1] — запускаем таймер, если выключился — останавливаем
            if (_kodOdNode?.Value == true)
            {
                if (!_timer.IsEnabled) StartTimer();
            }
            else
            {
                StopTimer();
            }

            RaisePropertyChanged(nameof(StartBrush));
            RaisePropertyChanged(nameof(OdBrush));
            RaisePropertyChanged(nameof(OodBrush));
            RaisePropertyChanged(nameof(IsOdOodEnabled));
        }

        // --- ЛОГИКА ТАЙМЕРА ---
        private void StartTimer()
        {
            _timeLeft = 10;
            UpdateOdText();
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            OdText = "ОД"; // Возвращаем базовый текст
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_timeLeft > 0)
            {
                _timeLeft--;
                UpdateOdText();
            }
            else
            {
                // Таймер дошел до 0. Сама кнопка останется желтой, пока KOD_OD[1] = true,
                // но отсчет прекратится (как в реальном пульте).
                _timer.Stop();
                if (_engine != null && _kodTodNode != null)
                {
                    _engine.InjectChange(_kodTodNode, true);
                }
            }
        }

        private void UpdateOdText()
        {
            OdText = $"ОД ({_timeLeft})";
        }

        // --- ХЕЛПЕРЫ ---
        private async void PulseNode(Node targetNode, string expectedName)
        {
            if (targetNode == null || _engine == null)
            {
                AppLogger.Log($"[TOP PANEL] ОШИБКА: Курок '{expectedName}' не найден!");
                return;
            }

            AppLogger.Log($"[TOP PANEL] Импульс команды: {targetNode.Id}");
            _engine.InjectChange(targetNode, true);
            await Task.Delay(500);
            _engine.InjectChange(targetNode, false);
        }

        private Node FindMkNode(Context ctx, Node parentNode, string targetPrefix)
        {
            if (parentNode?.LogicSource?.Groups != null)
            {
                foreach (var group in parentNode.LogicSource.Groups)
                {
                    if (group == null) continue;
                    var found = group.FirstOrDefault(n => n.Id.StartsWith(targetPrefix));
                    if (found != null) return found;
                }
            }
            // Фолбек по префиксу
            return ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith(targetPrefix));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}