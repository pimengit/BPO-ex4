using System;
using System.Collections.Generic;
using System.Linq;

namespace BPO_ex4.StationLogic
{
    public class SheetLogic
    {
        protected Node Self;

        // Было: protected Node[] X;
        // Стало: Массив списков. Каждый элемент массива соответствует одной "1" в шапке Excel.
        public List<Node>[] Groups;

        public TimeSpan OnDelay { get; protected set; } = TimeSpan.Zero;
        public TimeSpan OffDelay { get; protected set; } = TimeSpan.Zero;

        // Метод привязки теперь принимает структуру групп
        internal void Bind(Node self, List<Node>[] inputs)
        {
            Self = self;
            Groups = inputs;
            self.OnDelay = OnDelay;
            self.OffDelay = OffDelay;
        }

        // ==========================================
        // БАЗОВЫЕ ОПЕРАЦИИ
        // ==========================================

        // V(i) - Берет значение первой переменной в группе (для одиночных входов)
        protected bool V(int idx)
        {
            if (Groups == null || idx < 0 || idx >= Groups.Length) return false;

            var group = Groups[idx];
            return group != null && group.Count > 0 && group[0].Value;
        }

        protected bool OR(int idx)
        {
            if (Groups == null || idx < 0 || idx >= Groups.Length) return false;

            var group = Groups[idx];
            if (group == null) return false;

            foreach (var n in group)
                if (n.Value) return true;
            return false;
        }

        protected bool AND(int idx)
        {
            if (Groups == null || idx < 0 || idx >= Groups.Length) return false;

            var group = Groups[idx];
            if (group == null) return false;

            foreach (var n in group)
                if (!n.Value) return false;
            return true;
        }

        // !V(i) - Инверсия одиночного (синтаксический сахар)
        protected bool Not(int i) => !V(i);

        public virtual bool Compute()
        {
            return false;
        }
    }
}