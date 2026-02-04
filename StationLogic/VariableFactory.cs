using System;
using System.Collections.Generic;

namespace BPO_ex4.StationLogic
{
    public static class VariableFactory
    {
        public static Node Create(
            Context ctx,
            string sheet,
            int index,
            List<Node>[] inputGroups, // <--- Принимаем группы
            SheetLogic logic,
            string description)
        {
            var selfId = $"{sheet}[{index}]";
            var self = ctx.Get(selfId);
            //self.OnDelay = TimeSpan.Zero;
            //self.OffDelay = TimeSpan.FromMilliseconds(1);
            self.Description = description;

            // Гарантируем подписку на ВСЕ входы во ВСЕХ группах
            // i начинаем с 1, так как 0 индекс обычно пустой или служебный в вашей логике
            for (int i = 1; i < inputGroups.Length; i++)
            {
                var group = inputGroups[i];
                if (group == null) continue;

                foreach (var parentNode in group)
                {
                    if (parentNode == null) continue;
                    // Если родитель меняется -> эта нода должна пересчитаться
                    parentNode.Dependents.Add(self);
                }
            }

            logic.Bind(self, inputGroups);
            self.Compute = logic.Compute;

            return self;
        }
    }
}