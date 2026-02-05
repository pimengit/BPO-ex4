using System.Collections.Generic;
using BPO_ex4.StationLogic;

namespace BPO_ex4.ViewModels
{
    public class LogicRowViewModel
    {
        public int GroupIndex { get; set; }

        // Данные из LogicAnalyzer
        public string Operator { get; set; } // "OR", "AND", "V"
        public bool IsGroupInverted { get; set; } // Стоит ли "!" перед группой

        // Список переменных в этой группе
        public List<Node> Inputs { get; set; } = new List<Node>();

        // Для раскраски заголовка группы (Зеленый если группа активна)
        public bool IsActive { get; set; }
    }
}