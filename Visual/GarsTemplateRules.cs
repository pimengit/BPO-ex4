using System.Globalization;
using System.Xml.Linq;

namespace BPO_ex4.Visuals
{
    public static class GarsTemplateRules
    {
        public static double RectW { get; private set; } = 30;
        public static double RectH { get; private set; } = 14;

        public static double LineX { get; private set; } = 14;

        public static double Cap1X { get; private set; } = 0;
        public static double Cap1W { get; private set; } = 14;

        public static double Cap2X { get; private set; } = 15;
        public static double Cap2W { get; private set; } = 14;

        // Вызови этот метод ОДИН РАЗ при старте программы (например, в StationLoader)
        public static void Load(string pathToGarsXml)
        {
            if (!System.IO.File.Exists(pathToGarsXml)) return;

            var doc = XDocument.Load(pathToGarsXml);
            var elements = doc.Root?.Element("elements");
            if (elements == null) return;

            var culture = CultureInfo.InvariantCulture;

            var rect = elements.Elements("rect").FirstOrDefault(e => e.Attribute("id")?.Value == "1");
            if (rect != null)
            {
                RectW = double.Parse(rect.Attribute("w")?.Value ?? "30", culture);
                RectH = double.Parse(rect.Attribute("h")?.Value ?? "14", culture);
            }

            var line = elements.Elements("line").FirstOrDefault(e => e.Attribute("id")?.Value == "2");
            if (line != null)
            {
                LineX = double.Parse(line.Attribute("xbegin")?.Value ?? "14", culture);
            }

            var cap3 = elements.Elements("caption").FirstOrDefault(e => e.Attribute("id")?.Value == "3");
            if (cap3 != null)
            {
                Cap1X = double.Parse(cap3.Attribute("x")?.Value ?? "0", culture);
                Cap1W = double.Parse(cap3.Attribute("w")?.Value ?? "14", culture);
            }

            var cap4 = elements.Elements("caption").FirstOrDefault(e => e.Attribute("id")?.Value == "4");
            if (cap4 != null)
            {
                Cap2X = double.Parse(cap4.Attribute("x")?.Value ?? "15", culture);
                Cap2W = double.Parse(cap4.Attribute("w")?.Value ?? "14", culture);
            }
        }
    }
}