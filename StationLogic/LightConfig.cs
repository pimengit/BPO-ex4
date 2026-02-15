using System.Windows.Media;

namespace BPO_ex4.StationLogic
{
    public enum SignalColor
    {
        Red,
        Green,
        Yellow,
        White, // Лунно-белый
        Blue,
        Unknown
    }

    public static class SignalHelpers
    {
        // Парсинг русского текста из Excel
        public static SignalColor Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return SignalColor.Unknown;
            text = text.ToLower().Trim();

            if (text.Contains("красный")) return SignalColor.Red;
            if (text.Contains("зеленый") || text.Contains("зелёный")) return SignalColor.Green;
            if (text.Contains("желтый") || text.Contains("жёлтый")) return SignalColor.Yellow;
            if (text.Contains("белый") || text.Contains("лунно")) return SignalColor.White;
            if (text.Contains("синий")) return SignalColor.Blue;

            return SignalColor.Unknown;
        }

        // Конвертация в кисть WPF
        public static Brush ToBrush(this SignalColor color)
        {
            switch (color)
            {
                case SignalColor.Red: return Brushes.Red;
                case SignalColor.Green: return Brushes.Lime;
                case SignalColor.Yellow: return Brushes.Yellow;
                case SignalColor.White: return Brushes.WhiteSmoke;
                case SignalColor.Blue: return Brushes.Blue;
                default: return Brushes.Gray;
            }
        }
    }
}