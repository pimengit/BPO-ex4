using System;
using System.Collections.Generic;
using BPO_ex4.StationLogic;

namespace BPO_ex4.StationLogic
{
    public static class VariableFactory
    {
        public static Node Create(
            Context ctx,
            string sheet,
            int index,
            List<Node>[] inputGroups,
            SheetLogic logic,
            string description)
        {
            var selfId = $"{sheet}[{index}]";
            var self = ctx.Get(selfId);

            self.LogicSource = logic;
            self.Description = description;

            // === ВАЖНЕЙШАЯ ЧАСТЬ ДЛЯ СИМУЛЯЦИИ ===
            // Прописываем зависимости: "Если изменится вход -> пни меня"
            if (inputGroups != null)
            {
                for (int i = 1; i < inputGroups.Length; i++)
                {
                    var group = inputGroups[i];
                    if (group == null) continue;

                    foreach (var parentNode in group)
                    {
                        if (parentNode == null) continue;

                        if (!parentNode.Dependents.Contains(self))
                        {
                            parentNode.Dependents.Add(self);
                        }
                    }
                }
            }

            logic.Bind(self, inputGroups);

            // Если у логики есть реальный метод Compute, используем его
            self.Compute = logic.Compute;

            return self;
        }
    }
}