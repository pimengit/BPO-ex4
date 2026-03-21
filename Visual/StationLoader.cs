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
        private static void ParseBusyLinesRelative(System.Xml.Linq.XElement parentNode, ObservableCollection<BusyLineVM> targetCollection)
        {
            if (parentNode == null) return;
            var culture = CultureInfo.InvariantCulture;
            double bx = double.Parse(parentNode.Attribute("x")?.Value ?? "0", culture);
            double by = double.Parse(parentNode.Attribute("y")?.Value ?? "0", culture);

            foreach (var line in parentNode.Elements("line"))
            {
                double xb = double.Parse(line.Attribute("xbegin")?.Value ?? "0", culture) + bx;
                double yb = double.Parse(line.Attribute("ybegin")?.Value ?? "0", culture) + by;
                double xe = double.Parse(line.Attribute("xend")?.Value ?? "0", culture) + bx;
                double ye = double.Parse(line.Attribute("yend")?.Value ?? "0", culture) + by;
                targetCollection.Add(new BusyLineVM(xb, yb, xe, ye));
            }
        }


        // !!! Добавили аргумент SimulationEngine engine !!!
        public static ObservableCollection<VisualObjectViewModel> Load(string xmlPath, Context ctx, SimulationEngine engine)
        {
            var collection = new ObservableCollection<VisualObjectViewModel>();
            string dirRo = System.IO.Path.GetDirectoryName(xmlPath);
            string routeObjectsPath = System.IO.Path.Combine(dirRo, "routeobjects.xml");
            string routesXmlPath = System.IO.Path.Combine(dirRo, "routes.xml");
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

                    // Парсим толстые линии Плюса и Минуса для стрелки
                    ParseBusyLinesRelative(el.Element("busy_in_plus"), swVm.BusyInPlusLines);
                    ParseBusyLinesRelative(el.Element("busy_in_minus"), swVm.BusyInMinusLines);


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

                    // Если это стрелочная секция
                    if (isSwSection)
                    {
                        // ПАРСИМ ЛИНИИ МАГИСТРАЛИ (Они нужны для математики съездов)
                        var busyNode = el.Element("busy_line");
                        if (busyNode != null)
                        {
                            foreach (var line in busyNode.Elements("line"))
                            {
                                double xb = double.Parse(line.Attribute("xbegin")?.Value ?? "0", culture);
                                double yb = double.Parse(line.Attribute("ybegin")?.Value ?? "0", culture);
                                double xe = double.Parse(line.Attribute("xend")?.Value ?? "0", culture);
                                double ye = double.Parse(line.Attribute("yend")?.Value ?? "0", culture);

                                sectionVm.BusyLines.Add(new BusyLineVM(xb, yb, xe, ye));
                            }
                        }

                        // ПРИВЯЗЫВАЕМ СТРЕЛКИ К СЕКЦИИ
                        foreach (var swNode in el.Elements("switch"))
                        {
                            string swNumber = swNode.Attribute("number")?.Value;
                            var childSwitch = collection.OfType<SwitchViewModel>().FirstOrDefault(s => s.Name == swNumber);

                            if (childSwitch != null)
                            {
                                childSwitch.ParentSection = sectionVm;
                                // === ВАЖНО: Добавляем физическую стрелку в список секции ===
                                sectionVm.ChildSwitches.Add(childSwitch);
                            }

                            if (!string.IsNullOrEmpty(swNumber))
                            {
                                sectionVm.ChildSwitchNames.Add(swNumber);
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

                if (System.IO.File.Exists(routeObjectsPath))
                {
                    try
                    {
                        var routeDoc = XDocument.Load(routeObjectsPath);
                        var cultureR = CultureInfo.InvariantCulture;

                        // Ищем ИМЕННО теги <route_point>, как они записаны в твоем файле!
                        var pointElements = routeDoc.Descendants("route_point").ToList();

                        if (pointElements.Count == 0)
                        {
                            System.Windows.MessageBox.Show("Теги <route_point> не найдены!");
                        }

                        foreach (var el in pointElements)
                        {
                            double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                            double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);
                            string name = el.Attribute("name")?.Value ?? "???";

                            int number = 0;
                            int.TryParse(el.Attribute("id")?.Value, out number);

                            string type = el.Attribute("type")?.Value ?? "light";
                            bool isEnd = el.Attribute("as_end")?.Value == "true";

                            var ptVm = new RoutePointViewModel(x, y, name, number, type, isEnd);

                            // 1. Маршруты отсюда - это НАЧАЛЬНЫЕ маршруты для этой кнопки
                            foreach (var rNode in el.Elements("route"))
                            {
                                if (int.TryParse(rNode.Attribute("number")?.Value, out int rNum))
                                {
                                    ptVm.StartRoutes.Add(rNum);
                                }
                            }

                            // ВНИМАНИЕ: BindToLogic отсюда убрали!
                            collection.Add(ptVm);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        System.Windows.MessageBox.Show("Ошибка парсинга routeobjects.xml: " + ex.Message);
                    }
                }

                if (System.IO.File.Exists(routesXmlPath))
                {
                    var routesDoc = XDocument.Load(routesXmlPath);
                    foreach (var routeNode in routesDoc.Descendants("route"))
                    {
                        if (int.TryParse(routeNode.Attribute("number")?.Value, out int routeNum))
                        {
                            // === ДОБАВЛЕНО: Парсим путевые участки (type="21") ===
                            var sections = routeNode.Elements("object")
                                .Where(o => o.Attribute("type")?.Value == "21")
                                .Select(o => o.Attribute("name")?.Value)
                                .Where(n => !string.IsNullOrEmpty(n))
                                .ToList();

                            // Сохраняем секции в статический словарь
                            RoutePointViewModel.RouteSections[routeNum] = sections;

                            var switchStates = new Dictionary<string, bool>();
                            foreach (var obj in routeNode.Elements("object").Where(o => o.Attribute("type")?.Value == "10"))
                            {
                                string swName = obj.Attribute("name")?.Value;
                                var instr = obj.Element("instr");
                                if (swName != null && instr != null)
                                {
                                    // xor_mask="1" означает минусовое положение
                                    bool isMinus = instr.Attribute("xor_mask")?.Value == "1";
                                    switchStates[swName] = isMinus;
                                }
                            }
                            RoutePointViewModel.RouteSwitchStates[routeNum] = switchStates;

                            // (Старый код поиска конца маршрута)
                            var pointNode = routeNode.Element("point");
                            if (pointNode != null && int.TryParse(pointNode.Attribute("id")?.Value, out int pointId))
                            {
                                var ptVm = collection.OfType<RoutePointViewModel>().FirstOrDefault(p => p.Number == pointId);
                                if (ptVm != null) ptVm.EndRoutes.Add(routeNum);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show("Ошибка загрузки XML: " + ex.Message);
            }

            foreach (var ptVm in collection.OfType<RoutePointViewModel>())
            {
                ptVm.BindToLogic(ctx, engine);
            }

            return collection;
        }
    }
}