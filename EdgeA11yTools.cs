namespace Microsoft.Edge.A11y
{
    using Interop.UIAutomationCore;
    using OpenQA.Selenium;
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static ElementConverter;

    /// <summary>
    /// A set of helper methods which wrap accessibility APIs
    /// </summary>
    static class EdgeA11yTools
    {
        const int RETRIES = 5;
        const int RECURSIONDEPTH = 10;

        public const string ExceptionMessage = "Currently only Edge is supported";

        /// <summary>
        /// This is used to find the DOM of the browser, which is used as the starting
        /// point when searching for elements.
        /// </summary>
        /// <param name="retries">How many times we have already retried</param>
        /// <returns>The browser, or null if it cannot be found</returns>
        public static IUIAutomationElement FindBrowserDocument(int retries)
        {
            var uia = new CUIAutomation8();

            return FindBrowserDocumentRecurser(uia.GetRootElement(), uia) ?? (retries > RETRIES ? null : FindBrowserDocument(retries + 1));
        }

        /// <summary>
        /// This is used only internally to recurse through the elements in the UI tree
        /// to find the DOM.
        /// </summary>
        /// <param name="parent">The parent element to search from</param>
        /// <param name="uia">A UIAutomation8 element that serves as a walker factory</param>
        /// <returns>The browser, or null if it cannot be found</returns>
        static IUIAutomationElement FindBrowserDocumentRecurser(IUIAutomationElement parent, CUIAutomation8 uia)
        {
            try
            {
                var walker = uia.RawViewWalker;
                var element = walker.GetFirstChildElement(parent);

                for (element = walker.GetFirstChildElement(parent); element != null; element = walker.GetNextSiblingElement(element))
                {
                    var className = element.CurrentClassName;
                    var name = element.CurrentName;

                    if (name != null && name.Contains("Microsoft Edge"))
                    {
                        var result = FindBrowserDocumentRecurser(element, uia);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    if (className != null && className.Contains("Spartan"))
                    {
                        var result = FindBrowserDocumentRecurser(element, uia);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                    if (className != null && className == "TabWindowClass")
                    {
                        return element;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// This searches the UI tree to find an element with the given tag.
        /// </summary>
        /// <param name="browserElement">The browser element to search. </param>
        /// <param name="controlType">The tag to search for</param>
        /// <param name="searchStrategy">An alternative search strategy for elements which are not found by their controltype</param>
        /// <param name="foundControlTypes">A list of all control types found on the page, for error reporting</param>
        /// <returns>The elements found which match the tag given</returns>
        public static List<IUIAutomationElement> SearchChildren(
            IUIAutomationElement browserElement,
            UIAControlType controlType,
            Func<IUIAutomationElement, bool> searchStrategy,
            out HashSet<UIAControlType> foundControlTypes)
        {
            var uia = new CUIAutomation8();

            var walker = uia.RawViewWalker;
            var tosearch = new List<Tuple<IUIAutomationElement, int>>();
            var toreturn = new List<IUIAutomationElement>();
            foundControlTypes = new HashSet<UIAControlType>();

            //We use a 0 here to signify the depth in the BFS search tree. The root element will have depth of 0.
            tosearch.Add(new Tuple<IUIAutomationElement, int>(browserElement, 0));

            while (tosearch.Any(e => e.Item2 < RECURSIONDEPTH))
            {
                var current = tosearch.First().Item1;
                var currentdepth = tosearch.First().Item2;

                var convertedRole = GetControlTypeFromCode(current.CurrentControlType);
                foundControlTypes.Add(convertedRole);

                if (searchStrategy == null ? convertedRole == controlType : searchStrategy(current))
                {
                    toreturn.Add(current);
                }
                else
                {
                    for (var child = walker.GetFirstChildElement(current); child != null; child = walker.GetNextSiblingElement(child))
                    {
                        tosearch.Add(new Tuple<IUIAutomationElement, int>(child, currentdepth + 1));
                    }
                }

                tosearch.RemoveAt(0);
            }
            return toreturn;
        }

        /// <summary>
        /// Find a list of the elements on the page that can be found by 
        /// tabbing through the UI.
        /// </summary>
        /// <param name="driverManager">The WebDriver wrapper</param>
        /// <returns>A list of the ids of all tabbable elements</returns>
        public static List<string> TabbableIds(DriverManager driverManager)
        {
            const int timeout = 0;
            //add a dummy input element at the top of the page so we can start tabbing from there
            driverManager.ExecuteScript(
                "document.body.innerHTML = \"<input id='tabroot' />\" + document.body.innerHTML",
                timeout);

            var toreturn = new List<string>();
            var id = "";

            for (var i = 1; id != "tabroot"; i++)
            {
                if (id != "" && !toreturn.Contains(id))
                {
                    toreturn.Add(id);
                }

                driverManager.SendTabs("tabroot", i);

                var activeElement = driverManager.ExecuteScript("return document.activeElement", timeout) as IWebElement;
                if (activeElement != null) id = activeElement.GetAttribute("id");
            }

            //remove the dummy input so we don't influence any other tests
            driverManager.ExecuteScript(
                "document.body.removeChild(document.getElementById(\"tabroot\"))",
                timeout);
            return toreturn;
        }
    }

    /// <summary>
    /// Some extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Send to send tab keys to an element
        /// </summary>
        /// <param name="driver">The driver being extended</param>
        /// <param name="element">The element to send the tabs to</param>
        /// <param name="count">The number of times to send tab</param>
        public static void SendTabs(this DriverManager driver, string element, int count)
        {
            var tabs = new List<WebDriverKey>();
            for (var i = 0; i < count; i++)
            {
                tabs.Add(WebDriverKey.Tab);
            }
            driver.SendSpecialKeys(element, tabs);
        }


        /// <summary>
        /// A wrapper which converts strings with friendly-named special keys to be
        /// converted into the appropriate character codes.
        ///
        /// E.G. "Arrow_left" becomes '\uE012'.toString()
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="elementId"></param>
        /// <param name="keysToSend"></param>
        public static void SendSpecialKeys(this DriverManager driver, string elementId, List<WebDriverKey> keys)
        {
            var stringToSend = new StringBuilder();
            foreach (var key in keys)
            {
                if (key == WebDriverKey.Wait)
                {
                    System.Threading.Thread.Sleep(1000);
                    driver.SendKeys(elementId, stringToSend.ToString());
                    stringToSend = new StringBuilder();
                }
                else
                {
                    stringToSend.Append(GetWebDriverKeyString(key));
                }
            }
            driver.SendKeys(elementId, stringToSend.ToString());
        }

        public static void SendSpecialKeys(this DriverManager driver, string elementId, WebDriverKey key)
        {
            driver.SendSpecialKeys(elementId, new List<WebDriverKey> { key });
        }

        /// <summary>
        /// Get all of the Control Patterns supported by an element
        /// </summary>
        /// <param name="element">The element being extended</param>
        /// <param name="ids">The ids of the patterns</param>
        /// <returns>A list of all the patterns supported</returns>
        public static List<string> GetPatterns(this IUIAutomationElement element, out List<int> ids)
        {
            int[] inIds;
            string[] names;
            new CUIAutomation8().PollForPotentialSupportedPatterns(element, out inIds, out names);
            ids = inIds.ToList();
            return names.ToList();
        }

        /// <summary>
        /// Get all of the Control Patterns supported by an element
        /// </summary>
        /// <param name="element">The element being extended</param>
        /// <returns>A list of all the patterns supported</returns>
        public static List<string> GetPatterns(this IUIAutomationElement element)
        {
            var ids = new List<int>();
            return GetPatterns(element, out ids);
        }

        /// <summary>
        /// Get all the properties supported by an element
        /// </summary>
        /// <param name="element">The element being extended</param>
        /// <returns>A list of all the properties supported</returns>
        public static List<string> GetProperties(this IUIAutomationElement element)
        {
            int[] ids;
            string[] names;
            new CUIAutomation8().PollForPotentialSupportedProperties(element, out ids, out names);
            return names.ToList();
        }

        /// <summary>
        /// Get the names of all children (not all descendents)
        /// </summary>
        /// <param name="element">The element being extended</param>
        /// <returns>A list of all the children's names</returns>
        public static List<string> GetChildNames(this IUIAutomationElement element, Func<IUIAutomationElement, bool> searchStrategy = null)
        {
            var toReturn = new List<string>();
            var walker = new CUIAutomation8().RawViewWalker;
            for (var child = walker.GetFirstChildElement(element); child != null; child = walker.GetNextSiblingElement(child))
            {
                if (searchStrategy == null || searchStrategy(child))
                {
                    toReturn.Add(child.CurrentName);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Find all descendents of a given element, optionally searching with the given strategy
        /// </summary>
        /// <param name="element">The element whose descendents we need to find</param>
        /// <param name="searchStrategy">The strategy we should use to evaluate children</param>
        /// <returns>All elements that pass the searchStrategy</returns>
        public static List<IUIAutomationElement> GetAllDescendents(this IUIAutomationElement element, Func<IUIAutomationElement, bool> searchStrategy = null)
        {
            var toReturn = new List<IUIAutomationElement>();
            var walker = new CUIAutomation8().RawViewWalker;

            var toSearch = new Queue<IUIAutomationElement>();//BFS
            toSearch.Enqueue(element);

            while (toSearch.Any())//beware infinite recursion. If this becomes a problem add a limit
            {
                var current = toSearch.Dequeue();
                for (var child = walker.GetFirstChildElement(current); child != null; child = walker.GetNextSiblingElement(child))
                {
                    toSearch.Enqueue(child);
                }

                if (searchStrategy == null || searchStrategy(current))
                {
                    toReturn.Add(current);
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Parse the mystery object that is returned from WebDriver ExecuteScript calls.
        /// </summary>
        public static double ParseMystery(this object o)
        {
            try
            {
                return (int)o;
            }
            catch { }

            try
            {
                return (Int64)o;
            }
            catch { }

            try
            {
                return (double)o;
            }
            catch { }

            try
            {
                return double.Parse((string)o);
            }
            catch { }

            throw new InvalidCastException("Cannot parse to int, double, or string");
        }

        /// <summary>
        /// Takes a screenshot and saves in the current directory
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="name"></param>
        public static void Screenshot(this DriverManager driver, string name)
        {
            driver.GetScreenshot().SaveAsFile(Path.Combine(Directory.GetCurrentDirectory(), name + ".png"), ImageFormat.Png);
        }
    }
}
