using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public abstract class VisualObjectViewModel : INotifyPropertyChanged
    {
        protected Node _node;
        protected SimulationEngine _engine;

        public double X { get; set; }
        public double Y { get; set; }
        public int ZIndex { get; set; } = 0;
        public string Name { get; set; }

        public int Number { get; set; }

        public ICommand ClickCommand { get; private set; }
        public ICommand ZrCommand { get; private set; }


        public ICommand RightClickCommand { get; private set; }
        public ICommand LeftClickCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public VisualObjectViewModel()
        {
            ClickCommand = new SimpleCommand(ToggleValue);
            ZrCommand = new SimpleCommand(ToggleValue);
            RightClickCommand = new SimpleCommand(OnRightClick);
            LeftClickCommand = new SimpleCommand(OnLeftClick);
        }

        protected virtual void OnLeftClick()
        {
            // По дефолту (для секций) - просто инверсия одиночной ноды
           // if (_node != null && _engine != null)
             //   _engine.InjectChange(_node, !_node.Value);
        }

        protected virtual void OnRightClick()
        {
            // По дефолту ничего не делаем
        }

        // === ГЛАВНОЕ ИСПРАВЛЕНИЕ: Метод для вызова события из наследников ===
        protected void RaisePropertyChanged(string propertyName)
        {
            // Сразу делаем это в главном потоке, чтобы UI не ругался
            /*System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });*/
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                // Если propertyName == null => используем string.Empty (обозначает "всё")
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
            });
        
        }

        public virtual void BindToLogicSect(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            var childNodeSect = ctx.GetAllNodes()
                               .FirstOrDefault(n => n.Id.StartsWith("SECT_P") && n.Description == Name);




            if (childNodeSect != null && childNodeSect.LogicSource?.Groups != null)
            {
                foreach (var group in childNodeSect.LogicSource.Groups)
                {
                    if (group == null) continue;
                    var sectIn = group.FirstOrDefault(n => n.Id.StartsWith("SECT_IN"));
                    var relayKRK1 = group.FirstOrDefault(n => n.Id.StartsWith("RELAY_KRK1"));
                    if (sectIn != null)
                    {
                        _node = sectIn;
                        _node.Changed += (n) => OnLogicChanged();
                        OnLogicChanged();
                        return;
                    }
                    if (relayKRK1 != null)
                    {
                        _node = relayKRK1;
                        _node.Changed += (n) => OnLogicChanged();
                        OnLogicChanged();
                        return;
                    }
                }

            }
        }

        public virtual void BindToLogicZr(Context ctx, SimulationEngine engine)
        {
            _engine = engine; // Важно сохранить движок

            // Ищем переменную
            var foundNode = ctx.GetAllNodes()
                               .FirstOrDefault(n => n.Id.StartsWith("SIGGROUP_RI") && n.Description == Name);

            if (foundNode != null)
            {
                _node = foundNode; // ПРИСВАИВАЕМ В РОДИТЕЛЬСКОЕ ПОЛЕ _node
                _node.Changed += (n) => OnLogicChanged();
                OnLogicChanged();
            }
            else
            {
                // Если не нашли - можно лог или цвет ошибки
                OnLogicChanged();
            }
        }

        public virtual void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine; // Важно сохранить движок


           
        }



        private void ToggleValue()
        {
            if (_node != null && _engine != null)
            {
                AppLogger.Log($"CLICK: {Name} ({_node.Id})");
                _engine.InjectChange(_node, !_node.Value);

            }
        }

        // Базовая реакция: обновить всё (null)
        protected virtual void OnLogicChanged()
        {
            RaisePropertyChanged(null);
        }
    }

    public class SimpleCommand : ICommand
    {
        private readonly Action _action;
        public SimpleCommand(Action action) => _action = action;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action?.Invoke();
        public event EventHandler CanExecuteChanged;
    }
}