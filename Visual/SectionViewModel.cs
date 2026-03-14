using BPO_ex4.StationLogic;
using System.Linq;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class SectionViewModel : VisualObjectViewModel
    {
        // _node (из базового класса) будет отвечать за КЛИК (SECT_IN / KRK)

        private Node _occupancyNode; // Новая переменная: Отвечает за ЦВЕТ (SECT_P)
        private Node _pzNode;        // Зеленая полоса (Маршрут)
        private Node _lzNode;        // Ложная занятость
        private Node _lsNode;        // Ложная свободность

        public double Width { get; set; }
        public double Height { get; set; } = 8;

        public Brush FillColor
        {
            get
            {
                // 1. Если вообще ничего не нашли — Синий (Ошибка конфигурации)
                if (_occupancyNode == null) return Brushes.Blue;

                // 2. ПРОВЕРКА ЗАНЯТОСТИ (по SECT_P)
                // Если SECT_P == false (0), значит путь занят -> Красный
                // (При условии, что нет ложной занятости, или если LZ тоже влияет)

                bool isOccupied = !_occupancyNode.Value; // 0 = Занято, 1 = Свободно
                bool isLz = _lzNode != null && _lzNode.Value;
                bool isLs = _lsNode != null && _lsNode.Value;

                if (isOccupied)
                {
                    // Если занято и есть Ложная Занятость -> Желтый
                    if (isLz) return Brushes.Yellow;

                    // Иначе просто занято -> Красный
                    return Brushes.Red;
                }

                if (!isOccupied)
                {
                    // Если занято и есть Ложная Свободность -> Синий
                    if (isLs) return Brushes.Blue;

                    // Иначе свободно -> Светло серый
                    
                }

                // 3. Если МАРШРУТ (PZ == true) -> Зеленый
                if (_pzNode != null && _pzNode.Value) return Brushes.Lime;


                return Brushes.LightGray;


            }
        }

        public SectionViewModel(double x, double y, double w, string name)
        {
            X = x; Y = y; Width = w; Name = name;
        }

        public override void BindToLogicSect(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // ---------------------------------------------------------
            // ШАГ 1: Ищем главную переменную отображения (SECT_P)
            // ---------------------------------------------------------
            // Она нужна для раскраски (Красный/Серый)
            
                                            _occupancyNode = ctx.GetAllNodes()
                                .FirstOrDefault(n => n.Id.StartsWith("SECT_P") && (n.Description == Name || n.Description.Remove(0, 1) == Name));

            if (_occupancyNode != null)
            {
                // Подписываемся на изменение цвета
                _occupancyNode.Changed += _ => OnLogicChanged();
            }

            // ---------------------------------------------------------
            // ШАГ 2: Ищем переменную для КЛИКА (SECT_IN или RELAY_KRK)
            // ---------------------------------------------------------
            // Мы копаемся в исходниках SECT_P, чтобы найти, от чего она зависит
            if (_occupancyNode != null && _occupancyNode.LogicSource?.Groups != null)
            {
                foreach (var group in _occupancyNode.LogicSource.Groups)
                {
                    if (group == null) continue;

                    // Пытаемся найти входные переменные в этой группе
                    var inputNode = group.FirstOrDefault(n => n.Id.StartsWith("SECT_IN") || n.Id.StartsWith("RELAY_KRK") || n.Id.StartsWith("SECT_EI1"));

                    if (inputNode != null)
                    {
                        _node = inputNode; // Присваиваем в _node (базовый класс кликает по нему!)
                        break; // Нашли - выходим
                    }
                }
            }

            // Если через группы не нашли (бывает сложная логика), 
            // пробуем найти SECT_IN просто по имени (как запасной вариант)
            if (_node == null)
            {
                _node = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_IN") && n.Description == Name);
            }

            // ---------------------------------------------------------
            // ШАГ 3: Ищем доп. переменные (PZ, LZ)
            // ---------------------------------------------------------
            _pzNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_Pz") && n.Description == Name);
            _lzNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_Lz") && n.Description == Name);
            _lsNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_LS") && n.Description == Name);

            if (_pzNode != null) _pzNode.Changed += _ => OnLogicChanged();
            if (_lzNode != null) _lzNode.Changed += _ => OnLogicChanged();
            if (_lsNode != null) _lsNode.Changed += _ => OnLogicChanged();

            // Первичное обновление
            OnLogicChanged();
        }

        protected override void OnLogicChanged()
        {
            // Используем безопасный вызов из базового класса (или твой вариант с Dispatcher)
            RaisePropertyChanged(nameof(FillColor));
        }

        // Переопределяем клик (на всякий случай, если в базовом классе он не так работает)
        protected override void OnLeftClick()
        {
            // Меняем состояние _node (это SECT_IN), а цвет перерисуется, когда движок пересчитает SECT_P
            if (_node != null && _engine != null)
            {
                _engine.InjectChange(_node, !_node.Value);
            }
        }
    }
}