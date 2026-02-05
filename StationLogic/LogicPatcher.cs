using System;
using System.Collections.Generic;
using System.Linq;

namespace BPO_ex4.StationLogic
{
    public static class LogicPatcher
    {
        // Безопасное добавление входа в память
        public static void AddInputToRuntime(Node targetNode, int groupIndex, Node inputNode)
        {
            if (targetNode.LogicSource is SheetLogic logic)
            {
                // 1. Если массив групп вообще не создан — создаем
                if (logic.Groups == null)
                {
                    logic.Groups = new List<Node>[groupIndex + 1];
                }

                // 2. Если индекс выходит за пределы — расширяем массив
                if (groupIndex >= logic.Groups.Length)
                {
                    var newGroups = new List<Node>[groupIndex + 1];
                    // Копируем старые данные
                    Array.Copy(logic.Groups, newGroups, logic.Groups.Length);
                    logic.Groups = newGroups;
                }

                // 3. Если сама группа (список) пустая — инициализируем
                if (logic.Groups[groupIndex] == null)
                {
                    logic.Groups[groupIndex] = new List<Node>();
                }

                // 4. Добавляем ноду (защита от дублей)
                var group = logic.Groups[groupIndex];
                if (!group.Any(n => n.Id == inputNode.Id))
                {
                    group.Add(inputNode);
                }
            }
        }

        // Безопасное обновление входа
        public static void UpdateInputInRuntime(Node targetNode, int groupIndex, int inputIndex, Node newInputNode)
        {
            if (targetNode.LogicSource is SheetLogic logic)
            {
                if (logic.Groups == null) return;

                if (groupIndex < logic.Groups.Length && logic.Groups[groupIndex] != null)
                {
                    var group = logic.Groups[groupIndex];
                    if (inputIndex >= 0 && inputIndex < group.Count)
                    {
                        group[inputIndex] = newInputNode;
                    }
                }
            }
        }
    }
}