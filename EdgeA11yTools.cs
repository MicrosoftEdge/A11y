using Interop.UIAutomationCore;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.Edge.A11y.ElementConverter;

namespace Microsoft.Edge.A11y
{
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
        /// <returns>The first element found which matches the tag given</returns>
        public static IUIAutomationElement SearchChildren(
            IUIAutomationElement browserElement,
            UIAControlType controlType,
            out HashSet<UIAControlType> foundControlTypes)
        {
            var uia = new CUIAutomation8();

            var walker = uia.RawViewWalker;
            var tosearch = new List<Tuple<IUIAutomationElement, int>>();
            foundControlTypes = new HashSet<UIAControlType>();

            //We use a 0 here to signify the depth in the BFS search tree. The root element will have depth of 0.
            tosearch.Add(new Tuple<IUIAutomationElement, int>(browserElement, 0));

            while (tosearch.Any(e => e.Item2 < RECURSIONDEPTH))
            {
                var current = tosearch.First().Item1;
                var currentdepth = tosearch.First().Item2;

                var convertedRole = GetControlTypeFromCode(current.CurrentControlType);
                foundControlTypes.Add(convertedRole);

                if (convertedRole == controlType)
                {
                    return current;
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

            return null;
        }
    }

    /// <summary>
    /// Some extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Get all of the Control Patterns supported by an element
        /// </summary>
        /// <param name="element">The element being extended</param>
        /// <param name="ids">The ids of the patterns</param>
        /// <returns>A list of all the patterns supported</returns>
        public static List<UIAPattern> GetPatterns(this IUIAutomationElement element, out List<int> ids)
        {
            int[] inIds;
            string[] names;
            new CUIAutomation8().PollForPotentialSupportedPatterns(element, out inIds, out names);
            ids = inIds.ToList();
            return names.ToList().ConvertAll(n => (UIAPattern)Enum.Parse(typeof(UIAPattern), n));
        }

        /// <summary>
        /// Get all of the Control Patterns supported by an element
        /// </summary>
        /// <param name="element">The element being extended</param>
        /// <returns>A list of all the patterns supported</returns>
        public static List<UIAPattern> GetPatterns(this IUIAutomationElement element)
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
        /// Returns all children of a given element
        /// </summary>
        /// <param name="element">The element whose children to return</param>
        /// <returns>The children of the element</returns>
        public static List<IUIAutomationElement> Children(this IUIAutomationElement element)
        {
            var toReturn = new List<IUIAutomationElement>();
            var walker = new CUIAutomation8().RawViewWalker;
            for (var child = walker.GetFirstChildElement(element); child != null; child = walker.GetNextSiblingElement(child))
            {
                toReturn.Add(child);
            }

            return toReturn;
        }
    }
}
