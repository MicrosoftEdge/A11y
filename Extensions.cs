namespace Microsoft.Edge.A11y
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text;
    using static ElementConverter;
    using Interop.UIAutomationCore;

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
            var tabs = new List<WebDriverKey>(count);
            for (var i = 0; i < count; i++)
            {
                tabs.Add(WebDriverKey.Tab);
            }

            driver.SendSpecialKeys(element, tabs);
        }

        /// <summary>
        /// Allows sending special keys, including interstitial waits
        /// </summary>
        /// <param name="driver">The WebDriver instance</param>
        /// <param name="elementId">The html id of the element to which to send keys</param>
        /// <param name="keys">The keys to send</param>
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

        /// <summary>
        /// Send a single special key
        /// </summary>
        /// <param name="driver">The WebDriver instance</param>
        /// <param name="elementId">The html id of the element to which to send keys</param>
        /// <param name="key">The key to send</param>
        public static void SendSpecialKey(this DriverManager driver, string elementId, WebDriverKey key)
        {
            driver.SendSpecialKeys(elementId, new List<WebDriverKey>(1) { key });
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
            List<int> ids;
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
        /// Get the names of all children (not all descendants)
        /// </summary>
        /// <param name="element">The element being extended</param>
        /// <param name="searchStrategy">The custom method of searching for elements</param>
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
        /// Find all descendants of a given element, optionally searching with the given strategy
        /// </summary>
        /// <param name="element">The element whose descendants we need to find</param>
        /// <param name="searchStrategy">The strategy we should use to evaluate children</param>
        /// <returns>All elements that pass the searchStrategy</returns>
        public static List<IUIAutomationElement> GetAllDescendants(this IUIAutomationElement element, Func<IUIAutomationElement, bool> searchStrategy = null)
        {
            var toReturn = new List<IUIAutomationElement>();
            var walker = new CUIAutomation8().RawViewWalker;

            // Breadth-first search
            var toSearch = new Queue<IUIAutomationElement>();
            toSearch.Enqueue(element);

            // beware infinite recursion. If this becomes a problem add a limit
            while (toSearch.Any())
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
        /// <param name="o">The object to convert</param>
        /// <returns>A double if the object can be converted</returns>
        public static double ParseMystery(this object o)
        {
            if (o is int)
            {
                return (int)o;
            }

            if (o is long)
            {
                return (long)o;
            }

            if (o is double)
            {
                return (double)o;
            }

            double d;
            if (o is string && double.TryParse((string)o, out d))
            {
                return d;
            }

            throw new InvalidCastException("Cannot parse to int, double, or string");
        }

        /// <summary>
        /// Takes a screenshot and saves in the current directory
        /// </summary>
        /// <param name="driver">The WebDriver instance</param>
        /// <param name="name">The name to give to the screenshot</param>
        public static void Screenshot(this DriverManager driver, string name)
        {
            driver.GetScreenshot().SaveAsFile(Path.Combine(Directory.GetCurrentDirectory(), name + ".png"), ImageFormat.Png);
        }
    }
}
