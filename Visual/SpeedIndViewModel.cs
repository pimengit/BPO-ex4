using BPO_ex4.StationLogic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class SpeedIndViewModel : VisualObjectViewModel
    {
        // Основные переменные режима U
        private Node _u40, _u60, _u70, _u80;

        // Переменные ограничения OS
        private Node _os0, _os40, _os60, _os70;

        // Переменные отмены ограничения
        private Node _oos;

        // Командные переменные (импульсные)
        private Node _mk0, _mk40, _mk60, _mk70;
        private Node _mkOos; // Снятие ограничения

        // Свойства для интерфейса
        public double Width { get; set; } = 20; // Диаметр кружка по умолчанию
        public double Height { get; set; } = 20;
        public int Number { get; set; } // Если нужно где-то выводить номер

        private string _speedText = "";
        public string SpeedText => _speedText;

        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; RaisePropertyChanged(nameof(IsMenuOpen)); }
        }

        // Цвета
        public Brush BgColor => IsRestricted ? Brushes.White : Brushes.Transparent;
        public Brush BorderColor => IsRestricted ? Brushes.Red : Brushes.DarkGray;
        public Thickness BorderThick => IsRestricted ? new Thickness(2) : new Thickness(1);
        public Brush TextColor => IsRestricted ? Brushes.Red : Brushes.Black;

        private bool IsRestricted => (_os0?.Value == true) || (_os40?.Value == true) ||
                                     (_os60?.Value == true) || (_os70?.Value == true);

        // Команды контекстного меню
        public ICommand SetOS0Command { get; }
        public ICommand SetOS40Command { get; }
        public ICommand SetOS60Command { get; }
        public ICommand SetOS70Command { get; }

        public SpeedIndViewModel(double x, double y, string name, int number)
        {
            X = x;
            Y = y;
            Name = name;
            Number = number;
            ZIndex = 5; // Поверх путей

            SetOS0Command = new SimpleCommand(() => PulseNode(_mk0));
            SetOS40Command = new SimpleCommand(() => PulseNode(_mk40));
            SetOS60Command = new SimpleCommand(() => PulseNode(_mk60));
            SetOS70Command = new SimpleCommand(() => PulseNode(_mk70));
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // 1. Ищем основные переменные по имени кружка (Description == Name)
            _u40 = FindMainNode(ctx, "SPEED_40U");
            _u60 = FindMainNode(ctx, "SPEED_60U");
            _u70 = FindMainNode(ctx, "SPEED_70U");
            _u80 = FindMainNode(ctx, "SPEED_80U");

            _os0 = FindMainNode(ctx, "SPEED_OS0");
            _os40 = FindMainNode(ctx, "SPEED_OS40");
            _os60 = FindMainNode(ctx, "SPEED_OS60");
            _os70 = FindMainNode(ctx, "SPEED_OS70");

            _oos = FindMainNode(ctx, "SPEED_OOS");

            // 2. Ищем командные узлы ВНУТРИ логики OS
            _mk0 = FindDependency(_os0, "SPEED_OS0_MK");
            _mk40 = FindDependency(_os40, "SPEED_OS40_MK");
            _mk60 = FindDependency(_os60, "SPEED_OS60_MK");
            _mk70 = FindDependency(_os70, "SPEED_OS70_MK");

            // Узел отмены ищем внутри любой доступной OS переменной
            _mkOos = FindDependency(_oos, "SPEED_OOS_MK");

            // Подписываемся на изменения всех найденных узлов
            var allNodes = new[] { _u40, _u60, _u70, _u80, _os0, _os40, _os60, _os70 };
            foreach (var node in allNodes.Where(n => n != null))
            {
                node.Changed += (n) => OnLogicChanged();
            }

            OnLogicChanged();
        }

        protected override void OnLogicChanged()
        {
            // Определяем приоритетный текст
            if (IsRestricted)
            {
                if (_os0?.Value == true) _speedText = "0";
                else if (_os40?.Value == true) _speedText = "40";
                else if (_os60?.Value == true) _speedText = "60";
                else if (_os70?.Value == true) _speedText = "70";
            }
            else
            {
                // Ищем максимальную из включенных U
                if (_u80?.Value == true) _speedText = "80";
                else if (_u70?.Value == true) _speedText = "70";
                else if (_u60?.Value == true) _speedText = "60";
                else if (_u40?.Value == true) _speedText = "40";
                else _speedText = "0"; // Если ничего нет
            }

            // Обновляем UI
            RaisePropertyChanged(nameof(SpeedText));
            RaisePropertyChanged(nameof(BgColor));
            RaisePropertyChanged(nameof(BorderColor));
            RaisePropertyChanged(nameof(BorderThick));
            RaisePropertyChanged(nameof(TextColor));
        }

        protected override void OnLeftClick()
        {
            if (IsRestricted)
            {
                // Если ограничение уже есть -> снимаем его (импульс на OOS)
                PulseNode(_mkOos);
            }

        }

        protected override void OnRightClick()
        {
            if (!IsRestricted)
            {
                // Нет ограничения - вызываем меню
                IsMenuOpen = true;
            }
        }

        // Асинхронный метод импульса: Вкл -> ждем 0.5с -> Выкл
        private async void PulseNode(Node targetMkNode)
        {
            if (targetMkNode == null || _engine == null) return;

            AppLogger.Log($"[SPEED IND] Импульс команды: {targetMkNode.Id}");
            _engine.InjectChange(targetMkNode, true);

            await Task.Delay(500); // Полсекунды задержки

            _engine.InjectChange(targetMkNode, false);
            IsMenuOpen = false; // Закрываем меню после выбора
        }

        // --- ХЕЛПЕРЫ ---
        private Node FindMainNode(Context ctx, string prefix)
        {
            return ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith(prefix) && n.Description == Name);
        }

        private Node FindDependency(Node parentOS, string targetPrefix)
        {
            if (parentOS?.LogicSource?.Groups == null) return null;

            foreach (var group in parentOS.LogicSource.Groups)
            {
                if (group == null) continue;
                var found = group.FirstOrDefault(n => n.Id.StartsWith(targetPrefix));
                if (found != null) return found;
            }
            return null;
        }
    }
}