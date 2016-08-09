namespace Microsoft.Edge.A11y
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static ElementConverter;
    using Interop.UIAutomationCore;
    using OpenQA.Selenium;

    /// <summary>
    /// A set of helper methods which wrap accessibility APIs
    /// </summary>
    internal static class EdgeA11yTools
    {
        /// <summary>
        /// How many times to retry
        /// </summary>
        private const int RETRIES = 5;

        /// <summary>
        /// How many levels of recursion are allowed
        /// </summary>
        private const int RECURSIONDEPTH = 10;

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

            // We use a 0 here to signify the depth in the BFS search tree. The root element will have depth of 0.
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
            const int Timeout = 0;

            // add a dummy input element at the top of the page so we can start tabbing from there
            driverManager.ExecuteScript(
                "document.body.innerHTML = \"<input id='tabroot' />\" + document.body.innerHTML",
                Timeout);

            var toreturn = new List<string>();
            var id = string.Empty;

            for (var i = 1; id != "tabroot"; i++)
            {
                if (id != string.Empty && !toreturn.Contains(id))
                {
                    toreturn.Add(id);
                }

                driverManager.SendTabs("tabroot", i);

                var activeElement = driverManager.ExecuteScript("return document.activeElement", Timeout) as IWebElement;
                if (activeElement != null)
                {
                    id = activeElement.GetAttribute("id");
                }
            }

            // remove the dummy input so we don't influence any other tests
            driverManager.ExecuteScript(
                "document.body.removeChild(document.getElementById(\"tabroot\"))",
                Timeout);
            return toreturn;
        }

        /// <summary>
        /// This is used only internally to recurse through the elements in the UI tree
        /// to find the DOM.
        /// </summary>
        /// <param name="parent">The parent element to search from</param>
        /// <param name="uia">A UIAutomation8 element that serves as a walker factory</param>
        /// <returns>The browser, or null if it cannot be found</returns>
        private static IUIAutomationElement FindBrowserDocumentRecurser(IUIAutomationElement parent, CUIAutomation8 uia)
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
    }
}
