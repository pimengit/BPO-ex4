using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public class VSViewModel : VisualObjectViewModel
    {
        private List<Node> _VSNode = new List<Node>(); // Замыкающие реле / Искусственная разделка

        public double Width { get; set; }
        public double Height { get; set; } = 20;

        public Brush FillColor
        {
            get
            {
                // 1. Если не нашли ни одной привязки STAGE_VS -> Красный (Ошибка)
                if (_VSNode.Count == 0) return Brushes.Red;

                // 2. Если все STAGE_VS активны -> Зеленый, иначе -> Синий
                bool allActive = _VSNode.All(n => n.Value);
                return allActive ? Brushes.Blue : Brushes.Green;
            }
        }

        public VSViewModel(double x, double y, double w, string name)
        {
            X = x;
            Y = y;
            Width = w;
            Name = name;
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // Умный поиск: проверяем и точное совпадение, и совпадение с приставкой "ВС"
            var foundNodes = ctx.GetAllNodes()
                .Where(n => n.Id.StartsWith("STAGE_VS[") &&
                            !string.IsNullOrEmpty(n.Description) &&
                            (n.Description.ToLower() == Name.ToLower() ||
                             n.Description.ToLower() == Name.ToLower() + "ВС" ||
                             n.Description.ToLower() == Name.ToLower() + " ВС" ||
                             n.Description.Replace("ВС", "").Trim().ToLower() == Name.ToLower()))
                .ToList();

            if (foundNodes.Any())
            {
                _VSNode.AddRange(foundNodes);

                // На всякий случай кладем первую ноду в базовое поле _node
                _node = foundNodes.First();

                // Подписываемся на изменения каждой найденной переменной
                foreach (var vs in _VSNode)
                {
                    vs.Changed += (n) => OnLogicChanged();
                }
            }
            else
            {
                AppLogger.Log($"[VSViewModel] ВНИМАНИЕ: Не найдена логика STAGE_VS для кнопки '{Name}'");
            }

            // Первичное обновление цвета
            OnLogicChanged();
        }

        // === ВОЗДЕЙСТВУЕМ ПРЯМО НА STAGE_VS ПРИ КЛИКЕ ===
        protected override void OnLeftClick()
        {
            if (_engine != null && _VSNode.Any())
            {
                // Берем текущее значение первой ноды и инвертируем его
                bool newValue = !_VSNode.First().Value;

                AppLogger.Log($"CLICK VS: {Name} -> меняем состояние STAGE_VS на {newValue}");

                // Переключаем все найденные переменные
                foreach (var vs in _VSNode)
                {
                    _engine.InjectChange(vs, newValue);
                }
            }
        }

        protected override void OnLogicChanged()
        {
            // Используем твой безопасный метод из базового класса
            RaisePropertyChanged(nameof(FillColor));
        }
    }
}