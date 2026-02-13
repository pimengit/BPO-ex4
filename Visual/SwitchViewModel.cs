using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks; // Нужен для задержки (Task.Delay)
using System.Windows.Media;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public class SwitchViewModel : VisualObjectViewModel
    {
        // === КОНТРОЛЬ (Для цвета) ===
        private Node _pkNode; // Плюс (SWITCH_PK)
        private Node _mkNode; // Минус (SWITCH_MK)

        // === УПРАВЛЕНИЕ (Для записи) ===
        private Node _inPlusNode;
        private Node _inMinusNode;

        // === АВТОМАТИКА (Следим за ними) ===
        private Node _puNode; // Команда Плюс (SWITCH_PU)
        private Node _muNode; // Команда Минус (SWITCH_MU)

        public double Width { get; set; } = 20;
        public double Height { get; set; } = 20;

        public Brush FillColor
        {
            get
            {
                if (_pkNode == null || _mkNode == null) return Brushes.Gray;

                bool pk = _pkNode.Value;
                bool mk = _mkNode.Value;

                if (pk && !mk) return Brushes.Lime;      // Плюс
                if (!pk && mk) return Brushes.Yellow;    // Минус
                if (!pk && !mk) return Brushes.Red;      // Нет контроля
                return Brushes.Violet;                   // Взрез
            }
        }

        public SwitchViewModel(double x, double y, string name)
        {
            X = x; Y = y; Name = name; ZIndex = 10;
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // 1. Ищем КОНТРОЛЬ (PK, MK)
            _pkNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH_PK") && n.Description.Contains(Name));
            _mkNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH_MK") && n.Description.Contains(Name));

            if (_pkNode != null) _pkNode.Changed += (n) => OnLogicChanged();
            if (_mkNode != null) _mkNode.Changed += (n) => OnLogicChanged();

            // 2. Ищем АВТОМАТИКУ (PU, MU)
            _puNode = ctx.GetAllNodes().FirstOrDefault(n => Regex.IsMatch(n.Id, @"_PU\[\d\]$") && n.Description.Contains(Name));
            _muNode = ctx.GetAllNodes().FirstOrDefault(n => Regex.IsMatch(n.Id, @"_MU\[\d\]$") && n.Description.Contains(Name));

            // Подписываемся на команды
            if (_puNode != null) _puNode.Changed += OnAutoCommandChanged;
            if (_muNode != null) _muNode.Changed += OnAutoCommandChanged;

            // 3. Ищем УПРАВЛЕНИЕ (IN) внутри родителя SWITCH
            var parentSwitch = ctx.GetAllNodes()
                                  .FirstOrDefault(n => n.Id.StartsWith("SWITCH_PD") &&  n.Description.Contains(Name));

            if (parentSwitch != null && parentSwitch.LogicSource?.Groups != null)
            {
                foreach (var group in parentSwitch.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var node in group)
                    {
                        if (node.Id.StartsWith("SWITCH_IN"))
                        {
                            if (_inPlusNode == null) _inPlusNode = node;
                            else if (_inMinusNode == null) _inMinusNode = node;
                        }
                    }
                }
            }

            OnLogicChanged();
        }

        // === ОБРАБОТЧИК АВТОМАТИКИ ===
        private void OnAutoCommandChanged(Node commandNode)
        {
            // Реагируем только если команда пришла (стала true)
            if (commandNode.Value == true)
            {
                bool isPlusCommand = (commandNode == _puNode);
                // Запускаем асинхронный процесс перевода
                RunSwitchSequence(isPlusCommand);
            }
        }

        // Асинхронный метод перевода с задержкой
        private async void RunSwitchSequence(bool toPlus)
        {
            if (_engine == null || _inPlusNode == null || _inMinusNode == null) return;

            // Логируем
            string targetName = toPlus ? "ПЛЮС" : "МИНУС";
            AppLogger.Log($"AUTO SWITCH {Name}: Старт перевода в {targetName}");

            // ШАГ 1: Сброс контроля (0 0)
            _engine.InjectChange(_inPlusNode, false);
            _engine.InjectChange(_inMinusNode, false);

            // ШАГ 2: Ждем 1 секунду (1000 мс)
            // Task.Delay не блокирует интерфейс, программа продолжает работать
            await Task.Delay(1000);

            // ШАГ 3: Устанавливаем целевое состояние
            if (toPlus)
            {
                // Ставим 1 0
                _engine.InjectChange(_inPlusNode, true);
                _engine.InjectChange(_inMinusNode, false);
            }
            else
            {
                // Ставим 0 1
                _engine.InjectChange(_inPlusNode, false);
                _engine.InjectChange(_inMinusNode, true);
            }

            AppLogger.Log($"AUTO SWITCH {Name}: Перевод завершен");
        }

        // Ручное управление (ПКМ)
        protected override void OnRightClick()
        {
            if (_engine == null || _inPlusNode == null || _inMinusNode == null) return;

            bool isPlus = (_pkNode != null && _pkNode.Value);

            // Ручной перевод делаем мгновенно (или тоже можно через Sequence, если хочешь)
            if (isPlus)
            {
                _engine.InjectChange(_inPlusNode, false);
                _engine.InjectChange(_inMinusNode, true);
            }
            else
            {
                _engine.InjectChange(_inPlusNode, true);
                _engine.InjectChange(_inMinusNode, false);
            }
        }

        // Ручное управление (ЛКМ)
        protected override void OnLeftClick()
        {
            if (_engine == null) return;
            // Обрыв
            if (_inPlusNode != null) _engine.InjectChange(_inPlusNode, false);
            if (_inMinusNode != null) _engine.InjectChange(_inMinusNode, false);
        }

        protected override void OnLogicChanged()
        {
            RaisePropertyChanged(null);
        }
    }
}
