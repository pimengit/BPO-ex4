using System;
using System.Collections.Generic;
using System.Linq;

namespace BPO_ex4.StationLogic
{
    public abstract class SheetLogic
    {
        protected Node Self;

        // Было: protected Node[] X;
        // Стало: Массив списков. Каждый элемент массива соответствует одной "1" в шапке Excel.
        protected List<Node>[] Groups;

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
        protected bool V(int i)
        {
            if (Groups[i].Count == 0) return false; // Защита от пустых
            return Groups[i][0].Value;
        }

        // OR(i) - Логическое СЛОЖЕНИЕ (ИЛИ) всех переменных в группе i
        // Соответствует значку "Сумма" в ТТ
        protected bool OR(int i)
        {
            var group = Groups[i];
            for (int k = 0; k < group.Count; k++)
            {
                if (group[k].Value) return true;
            }
            return false;
        }

        // AND(i) - Логическое УМНОЖЕНИЕ (И) всех переменных в группе i
        // Соответствует конструкции, когда все элементы должны быть 1
        protected bool AND(int i)
        {
            var group = Groups[i];
            for (int k = 0; k < group.Count; k++)
            {
                if (!group[k].Value) return false;
            }
            return true;
        }

        // !V(i) - Инверсия одиночного (синтаксический сахар)
        protected bool Not(int i) => !V(i);

        public abstract bool Compute();
    }
}