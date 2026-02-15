using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Globalization;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public static class StationLoader
    {
        // !!! Добавили аргумент SimulationEngine engine !!!
        public static ObservableCollection<VisualObjectViewModel> Load(string xmlPath, Context ctx, SimulationEngine engine)
        {
            var collection = new ObservableCollection<VisualObjectViewModel>();

            try
            {
                var doc = XDocument.Load(xmlPath);
                var culture = CultureInfo.InvariantCulture;

                // 1. ПАРСИНГ СЕКЦИЙ
                var trackElements = doc.Descendants("tracks")
                                       .Elements("section")
                                       .Elements("object");

                var zrElements = doc.Descendants("sig_group")
                                    .Elements("object");

                // 3. СТРЕЛКИ
                var switchElements = doc.Descendants("switch").Elements("object");
                // Если в XML просто <object ... type="switch"> то искать надо иначе, 
                // но судя по tracks->section, у тебя структура по типам папок.

                var garsElements = doc.Descendants("gars_indicators")
                      .Elements("object");


                var lightElements = doc.Descendants("x_light")
                       .Elements("object");

                foreach (var el in lightElements)
                {
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);
                    string name = el.Attribute("name")?.Value ?? "???";

                    // Считываем Number (ОБЯЗАТЕЛЬНО, это связь с OKSE)
                    int number = int.Parse(el.Attribute("number")?.Value ?? "0");

                    // Направление (odd/even) - пригодится для рисования мачты
                    string dir = el.Attribute("dir")?.Value ?? "odd";

                    var lightVm = new LightViewModel(x, y, number, name, dir);
                    lightVm.BindToLogic(ctx, engine);

                    collection.Add(lightVm);
                }

                foreach (var el in garsElements)
                {
                    // Парсим атрибуты
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);

                    // Ширина может быть width, dw или w
                    double w = double.Parse(el.Attribute("width")?.Value ?? el.Attribute("dw")?.Value ?? "60", culture);

                    // Имя для связки с переменными
                    string name = el.Attribute("name")?.Value ?? "???";
                    int number = int.Parse(el.Attribute("number")?.Value ?? "???");

                    var garsVm = new GarsViewModel(x, y, w, name, number);
                    garsVm.BindToLogic(ctx, engine);

                    collection.Add(garsVm);
                }

                foreach (var el in switchElements)
                {
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);
                    string name = el.Attribute("name")?.Value ?? "???";
                    var swVm = new SwitchViewModel(x, y, name);
                    swVm.BindToLogic(ctx, engine);
                    collection.Add(swVm);
                }

                foreach (var el in trackElements)
                {
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);
                    double dw = double.Parse(el.Attribute("dw")?.Value ?? "60", culture);
                    string name = el.Attribute("name")?.Value ?? "???";

                    var sectionVm = new SectionViewModel(x, y, dw, name);

                    // !!! Передаем движок в метод привязки !!!
                    sectionVm.BindToLogicSect(ctx, engine);

                    collection.Add(sectionVm);
                }
                foreach (var z in zrElements)
                {
                    double x = double.Parse(z.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(z.Attribute("y")?.Value ?? "0", culture);
                    double dw = 30;
                    string name = z.Attribute("name")?.Value ?? "???";

                    var zrVm = new ZrViewModel(x, y, dw, name);

                    // !!! Передаем движок в метод привязки !!!
                    zrVm.BindToLogicZr(ctx, engine);

                    collection.Add(zrVm);
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка загрузки XML: " + ex.Message);
            }

            return collection;
        }
    }
}