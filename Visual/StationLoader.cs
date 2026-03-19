using BPO_ex4.StationLogic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Media;
using System.Xml.Linq;

namespace BPO_ex4.Visuals
{
    public static class StationLoader
    {

        private static void ParseLines(System.Xml.Linq.XElement parentNode, System.Collections.ObjectModel.ObservableCollection<System.Windows.Media.LineGeometry> targetCollection)
        {
            if (parentNode == null) return;
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            double offsetX = double.Parse(parentNode.Attribute("x")?.Value ?? "0", culture);
            double offsetY = double.Parse(parentNode.Attribute("y")?.Value ?? "0", culture);

            foreach (var line in parentNode.Elements("line"))
            {
                double xb = double.Parse(line.Attribute("xbegin")?.Value ?? "0", culture) + offsetX;
                double yb = double.Parse(line.Attribute("ybegin")?.Value ?? "0", culture) + offsetY;
                double xe = double.Parse(line.Attribute("xend")?.Value ?? "0", culture) + offsetX;
                double ye = double.Parse(line.Attribute("yend")?.Value ?? "0", culture) + offsetY;
                targetCollection.Add(new System.Windows.Media.LineGeometry(new System.Windows.Point(xb, yb), new System.Windows.Point(xe, ye)));
            }
        }
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

                // 2. ЗР
                var zrElements = doc.Descendants("sig_group")
                                    .Elements("object");

                

                // 3. СТРЕЛКИ
                var switchElements = doc.Descendants("switch").Elements("object");
                // Если в XML просто <object ... type="switch"> то искать надо иначе, 
                // но судя по tracks->section, у тебя структура по типам папок.

                // 4. ГАРС
                var garsElements = doc.Descendants("gars_indicators")
                      .Elements("object");

                // 5. Светофоры
                var lightElements = doc.Descendants("x_light")
                       .Elements("object");

                // 6. АРС/АБ
                var VSElements = doc.Descendants("traffic_mode")
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
                    // Парсим координаты
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);

                    // Если ширины нет в XML, ставим 0 (тогда возьмется ширина из файла правил)
                    double w = double.Parse(el.Attribute("width")?.Value ?? el.Attribute("dw")?.Value ?? "0", culture);

                    string name = el.Attribute("name")?.Value ?? "???";

                    // Безопасный парсинг номера
                    int number = 0;
                    int.TryParse(el.Attribute("number")?.Value, out number);

                    // Теперь передаем 5 аргументов
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



                    // Считываем точки для ПЛЮСА
                    var plusNode = el.Element("plus");
                    if (plusNode != null)
                    {
                        foreach (var pt in plusNode.Elements("point"))
                        {
                            double px = double.Parse(pt.Attribute("x")?.Value ?? "0", culture);
                            double py = double.Parse(pt.Attribute("y")?.Value ?? "0", culture);
                            swVm.PlusPoints.Add(new System.Windows.Point(px, py));
                        }
                    }

                    // Считываем точки для МИНУСА
                    var minusNode = el.Element("minus");
                    if (minusNode != null)
                    {
                        foreach (var pt in minusNode.Elements("point"))
                        {
                            double px = double.Parse(pt.Attribute("x")?.Value ?? "0", culture);
                            double py = double.Parse(pt.Attribute("y")?.Value ?? "0", culture);
                            swVm.MinusPoints.Add(new System.Windows.Point(px, py));
                        }
                    }

                    

                    // Парсим всё одним махом!
                    ParseLines(el.Element("closed_in_plus"), swVm.ControlPlusLines);
                    ParseLines(el.Element("closed_in_minus"), swVm.ControlMinusLines);

              

                    // Достаем короткие корешки!
                    ParseLines(el.Element("closed_in_plus"), swVm.ControlPlusLines);
                    ParseLines(el.Element("closed_in_minus"), swVm.ControlMinusLines);

                    // Координаты квадратика с цифрой
                    var rectNode = el.Element("rect");
                    if (rectNode != null)
                    {
                        swVm.RectX = double.Parse(rectNode.Attribute("x")?.Value ?? "0", culture);
                        swVm.RectY = double.Parse(rectNode.Attribute("y")?.Value ?? "0", culture);
                    }

                    // ... остальной парсинг PlusPoints / MinusPoints ...

                    swVm.BindToLogic(ctx, engine);
                    collection.Add(swVm);
                }

                foreach (var el in trackElements)
                {
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);

                    // Если dw нет, ставим 0 (а не 60, чтобы не было "призраков")
                    double dw = double.Parse(el.Attribute("dw")?.Value ?? "0", culture);
                    string name = el.Attribute("name")?.Value ?? "???";

                    // Проверяем, стрелочная ли это секция
                    bool isSwSection = el.Attribute("sw_section")?.Value == "1";
                    var sectionVm = new SectionViewModel(x, y, dw, name, isSwSection);

                    // Если это стрелочная секция, парсим её контур (lock)
                    if (isSwSection)
                    {
                        foreach (var swNode in el.Elements("switch"))
                        {
                            string swNumber = swNode.Attribute("number")?.Value;
                            // Находим созданную ранее стрелку с этим номером (или именем)
                            var childSwitch = collection.OfType<SwitchViewModel>().FirstOrDefault(s => s.Name == swNumber);
                            if (childSwitch != null)
                            {
                                // Говорим стрелке, кто её родительская секция
                                childSwitch.ParentSection = sectionVm;
                            }
                        }
                    }

                    // Привязка логики
                    sectionVm.BindToLogicSect(ctx, engine);

                    collection.Add(sectionVm);
                }
                foreach (var z in zrElements)
                {
                    double x = double.Parse(z.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(z.Attribute("y")?.Value ?? "0", culture);
                    double dw = 40;
                    string name = z.Attribute("name")?.Value ?? "???";

                    var zrVm = new ZrViewModel(x, y, dw, name);

                    // !!! Передаем движок в метод привязки !!!
                    zrVm.BindToLogicZr(ctx, engine);

                    collection.Add(zrVm);
                }
                foreach (var z in VSElements)
                {
                    double x = double.Parse(z.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(z.Attribute("y")?.Value ?? "0", culture);
                    double dw = 40;
                    string name = z.Attribute("name")?.Value ?? "???";

                    var VSVm = new VSViewModel(x, y, dw, name);

                    // !!! Передаем движок в метод привязки !!!
                    VSVm.BindToLogic(ctx, engine);

                    collection.Add(VSVm);
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