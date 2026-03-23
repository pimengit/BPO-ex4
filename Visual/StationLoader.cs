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

                var trackElements = doc.Descendants("tracks").Elements("section").Elements("object");
                var zrElements = doc.Descendants("sig_group").Elements("object");
                var switchElements = doc.Descendants("switch").Elements("object");
                var garsElements = doc.Descendants("gars_indicators").Elements("object");
                var lightElements = doc.Descendants("x_light").Elements("object");
                var VSElements = doc.Descendants("traffic_mode").Elements("object");

                foreach (var el in lightElements)
                {
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);
                    string name = el.Attribute("name")?.Value ?? "???";
                    int number = int.Parse(el.Attribute("number")?.Value ?? "0");
                    string dir = el.Attribute("dir")?.Value ?? "odd";

                    var lightVm = new LightViewModel(x, y, number, name, dir);
                    lightVm.BindToLogic(ctx, engine);
                    collection.Add(lightVm);
                }

                foreach (var el in garsElements)
                {
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);
                    double w = double.Parse(el.Attribute("width")?.Value ?? el.Attribute("dw")?.Value ?? "0", culture);
                    string name = el.Attribute("name")?.Value ?? "???";
                    int number = 0;
                    int.TryParse(el.Attribute("number")?.Value, out number);

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

                    ParseLines(el.Element("closed_in_plus"), swVm.ControlPlusLines);
                    ParseLines(el.Element("closed_in_minus"), swVm.ControlMinusLines);

                    // УДАЛЕН ПАРСИНГ busy_in_plus И busy_in_minus!

                    var rectNode = el.Element("rect");
                    if (rectNode != null)
                    {
                        swVm.RectX = double.Parse(rectNode.Attribute("x")?.Value ?? "0", culture);
                        swVm.RectY = double.Parse(rectNode.Attribute("y")?.Value ?? "0", culture);
                    }

                    swVm.BindToLogic(ctx, engine);
                    collection.Add(swVm);
                }

                foreach (var el in trackElements)
                {
                    double x = double.Parse(el.Attribute("x")?.Value ?? "0", culture);
                    double y = double.Parse(el.Attribute("y")?.Value ?? "0", culture);
                    double dw = double.Parse(el.Attribute("dw")?.Value ?? "0", culture);
                    string name = el.Attribute("name")?.Value ?? "???";
                    bool isSwSection = el.Attribute("sw_section")?.Value == "1";

                    var sectionVm = new SectionViewModel(x, y, dw, name, isSwSection);


                    // Если это стрелочная секция, парсим её контур (lock)
                    // Если это стрелочная секция, парсим её контур (lock)
                    if (isSwSection)
                    {
                        // === ВОССТАНОВЛЕНО: Парсим сам контур стрелки! Без него её не видно! ===
                        var lockNode = el.Element("lock");
                        if (lockNode != null)
                        {
                            foreach (var pt in lockNode.Elements("point"))
                            {
                                double px = double.Parse(pt.Attribute("x")?.Value ?? "0", culture);
                                double py = double.Parse(pt.Attribute("y")?.Value ?? "0", culture);
                                sectionVm.LockPoints.Add(new System.Windows.Point(px, py));
                            }
                        }

                        // ПАРСИМ ТОЛСТЫЕ ЛИНИИ
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

                        // ПРИВЯЗЫВАЕМ СТРЕЛКИ ДЛЯ ПРЕДСКАЗАНИЯ
                        foreach (var swNode in el.Elements("switch"))
                        {
                            string swNumber = swNode.Attribute("number")?.Value;
                            var childSwitch = collection.OfType<SwitchViewModel>().FirstOrDefault(s => s.Name == swNumber);

                            if (childSwitch != null)
                            {
                                childSwitch.ParentSection = sectionVm;

                                // === ИСПРАВЛЕНИЕ №1: ЗАСТАВЛЯЕМ СТРЕЛКУ КРАСНЕТЬ ===
                                // Когда секция меняет цвет, дергаем стрелку, чтобы она тоже обновилась!
                                sectionVm.PropertyChanged += (sender, args) =>
                                {
                                    if (args.PropertyName == "FillColor")
                                    {
                                        childSwitch.UpdateColor(); // <--- ИСПРАВЛЕНО ЗДЕСЬ!
                                    }
                                };
                            }

                            if (!string.IsNullOrEmpty(swNumber))
                            {
                                sectionVm.ChildSwitchNames.Add(swNumber);
                            }
                        }
                    }

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
                    VSVm.BindToLogic(ctx, engine);
                    collection.Add(VSVm);
                }

                if (System.IO.File.Exists(routeObjectsPath))
                {
                    try
                    {
                        var routeDoc = XDocument.Load(routeObjectsPath);
                        var cultureR = CultureInfo.InvariantCulture;
                        var pointElements = routeDoc.Descendants("route_point").ToList();

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

                            foreach (var rNode in el.Elements("route"))
                            {
                                if (int.TryParse(rNode.Attribute("number")?.Value, out int rNum))
                                {
                                    ptVm.StartRoutes.Add(rNum);
                                }
                            }
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
                            var sections = routeNode.Elements("object")
                                .Where(o => o.Attribute("type")?.Value == "21")
                                .Select(o => o.Attribute("name")?.Value)
                                .Where(n => !string.IsNullOrEmpty(n))
                                .ToList();

                            RoutePointViewModel.RouteSections[routeNum] = sections;

                            var switchStates = new Dictionary<string, bool>();
                            foreach (var obj in routeNode.Elements("object").Where(o => o.Attribute("type")?.Value == "10"))
                            {
                                string swName = obj.Attribute("name")?.Value;
                                var instr = obj.Element("instr");
                                if (swName != null && instr != null)
                                {
                                    bool isMinus = instr.Attribute("xor_mask")?.Value == "1";
                                    switchStates[swName] = isMinus;
                                }
                            }
                            RoutePointViewModel.RouteSwitchStates[routeNum] = switchStates;

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