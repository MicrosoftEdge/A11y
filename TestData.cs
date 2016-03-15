using Interop.UIAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Edge.A11y
{

    /// <summary>
    /// This is where the logic of the tests is stored
    /// </summary>
    public class TestData
    {
        /// <summary>
        /// The name of the test, which corresponds to the name of the html element
        /// </summary>
        public string _TestName;
        /// <summary>
        /// The name of the UIA control type we will use to search for the element
        /// </summary>
        public string _ControlType;
        /// <summary>
        /// The name of the UIA localized control type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string _LocalizedControlType;
        /// <summary>
        /// The name of the UIA landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string _LandmarkType;
        /// <summary>
        /// The name of the UIA localized landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string _LocalizedLandmarkType;
        /// <summary>
        /// A list of ids for all the elements that should be keyboard accessible (via tab)
        /// </summary>
        public List<string> _KeyboardElements;
        /// <summary>
        /// If not null, this func will be run as part of the test. If it returns false,
        /// the test will score 50%; if true, 100%
        /// </summary>
        public Func<List<IUIAutomationElement>, DriverManager, List<string>, bool> _AdditionalRequirement;
        /// <summary>
        /// If not null, this func will be used to test elements to see if they should be
        /// tested (instead of matching _ControlType).
        /// </summary>
        public Func<IUIAutomationElement, bool> _SearchStrategy;

        /// <summary>
        /// Simple ctor
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="controlType"></param>
        /// <param name="localizedControlType"></param>
        /// <param name="landmarkType"></param>
        /// <param name="localizedLandmarkType"></param>
        /// <param name="keyboardElements"></param>
        /// <param name="additionalRequirement"></param>
        public TestData(string testName,
            string controlType,
            string localizedControlType = null,
            string landmarkType = null,
            string localizedLandmarkType = null,
            List<string> keyboardElements = null,
            Func<List<IUIAutomationElement>, DriverManager, List<string>, bool> additionalRequirement = null,
            Func<IUIAutomationElement, bool> searchStrategy = null)
        {
            _TestName = testName;
            _ControlType = controlType;
            _LocalizedControlType = localizedControlType;
            _LandmarkType = landmarkType;
            _LocalizedLandmarkType = localizedLandmarkType;
            _KeyboardElements = keyboardElements;
            _AdditionalRequirement = additionalRequirement;
            _SearchStrategy = searchStrategy;
        }

        //All the tests to run
        public static Lazy<List<TestData>> alltests = new Lazy<List<TestData>>(AllTests);

        /// <summary>
        /// Get the TestData for the given test page
        /// </summary>
        /// <param name="testName">The name of the file being tested</param>
        /// <returns>TestData for the given test page, or null if it couldn't be found</returns>
        public static TestData DataFromName(string testName)
        {
            return alltests.Value.FirstOrDefault(t => t._TestName == testName);
        }

        /// <summary>
        /// Singleton initializer
        /// </summary>
        /// <returns></returns>
        static List<TestData> AllTests()
        {
            var converter = new ElementConverter();
            const int timeout = 0;
            var alltests = new List<TestData>{
                new TestData("article", "Document", "article"),
                new TestData("aside", "Group", "aside", "Custom", "complementary"),
                new TestData("audio", "Group", "audio",
                    additionalRequirement: CheckChildNames(
                        new List<string> {
                                "Play",
                                "Time elapsed",
                                "Seek",
                                "Time remaining",
                                "Mute",
                                "Volume"})),//TODO get full list when it's decided
                new TestData("canvas", "Image"),
                new TestData("datalist", "Edit", keyboardElements: new List<string> { "input1" },
                    additionalRequirement: ((elements, driver, ids) => elements.All(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0))),
                new TestData("details", null),
                new TestData("dialog", null),
                new TestData("figure", "Group", "figure"),
                new TestData("figure-figcaption", "Image",
                    additionalRequirement: ((elements, driver, ids) => elements.All(element => element.CurrentName == "HTML5 logo caption"))),
                new TestData("footer", "Group", "footer", "Custom", "content information"),
                new TestData("header", "Group", "header", "Custom", "banner"),
                new TestData("input-color", "Edit", "color picker"),
                new TestData("input-date", "Calendar", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("input-email", "Edit", "email", keyboardElements: new List<string> { "input1", "input2" }, additionalRequirement: CheckValidation()),
                new TestData("input-month", "Calendar", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("input-number", "Edit", "number", keyboardElements: new List<string> { "input1", "input2" }, additionalRequirement: CheckValidation()),
                new TestData("input-range", "Slider", keyboardElements: new List<string> { "input1", "input2" }),//need clarification
                new TestData("input-search", "Edit", "search", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("input-tel", "Edit", "telephone", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("input-time", "Spinner", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("input-url", "Edit", "url", keyboardElements: new List<string> { "input1", "input2" }, additionalRequirement: CheckValidation()),
                new TestData("input-week", "Calendar", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("main", "Group", "main", "Main", "main"),
                new TestData("mark", "Text"),
                new TestData("meter", "Progressbar", "meter", keyboardElements: new List<string> { "meter1" },
                    additionalRequirement: ((elements, driver, ids) => elements.All(element => element.GetProperties().Contains("IsReadOnly"))),
                    searchStrategy: (element => element.GetPatterns().Contains("RangeValuePattern"))),//NB the ControlType is not used for searching this element
                new TestData("menuitem", null),
                new TestData("menupopup", null),
                new TestData("menutoolbar", null),
                new TestData("nav", "Group", "navigation", "Navigation", "navigation"),
                new TestData("output", "Group"),
                new TestData("progress", "Progressbar"),
                new TestData("section", "Text", "section", "Custom", "region"),
                new TestData("summary", null),
                //alltests.Add(new TestData("time", "time"));//currently marked as ? in the spec
                new TestData("track", "track",
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        driver.ExecuteScript(Javascript.Track, timeout);

                        return (bool)driver.ExecuteScript("return Modernizr.track && Modernizr.texttrackapi", timeout);
                    }),
                    searchStrategy: (element => true)),
                new TestData("video", "Group", null, keyboardElements: new List<string> { "video1" },
                    additionalRequirement:
                        CheckChildNames(
                            new List<string> {
                                    "Play",
                                    "Time elapsed",
                                    "Seek",
                                    "Time remaining",
                                    "Zoom in",
                                    "Show audio",
                                    "Show captioning",
                                    "Mute",
                                    "Volume",
                                    "Full screen" })),//TODO get full list when it's decided
                new TestData("hidden-att", "Button", null,
                    additionalRequirement: ((elements, driver, ids) => elements.Count(e => e.CurrentControlType == converter.GetElementCodeFromName("Button")) == 1 && !ids.Any()),
                    searchStrategy: (element => element.CurrentControlType != converter.GetElementCodeFromName("Pane"))),//take all elements other than the window
                new TestData("required-att", "Edit",
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        driver.SendSubmit("input1");
                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        return elements.Count() == 1 && elements.All(
                                                            e => e.CurrentControllerFor != null
                                                            && e.CurrentControllerFor.Length > 0
                                                            && e.CurrentIsDataValidForForm == 0);
                    }),
                new TestData("placeholder-att", "Edit",
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        if (elements.Count() != 6)
                        {
                            return false;
                        }

                        var elementNames = elements.Select(element => element.CurrentName).ToList();
                        return new List<Func<List<string>, bool>>{
                            names => names.Contains("placeholder text 1"),
                            names => names.Contains("Label text 2:"),
                            names => names.Contains("Label text 3:"),
                            names => names.Contains("placeholder text 4"),
                            names => names.Contains("placeholder text 5"),
                            names => names.Contains(""),
                        }.All(f => f(elementNames));
                    }))
                //TODO add Description check once it's been figured out
            };

            return alltests;
        }

        /// <summary>
        /// Func factory for checking that when invalid input is entered into a form,
        /// an error message appears.
        /// </summary>
        /// <returns></returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, bool> CheckValidation()
        {
            return (elements, driver, ids) =>
                {
                    for (var i = 0; i < elements.Count; i++)
                    {
                        driver.SendKeys("input" + (i + 1), "invalid");
                        driver.SendSubmit("input" + (i + 1));
                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        if (!(elements[i].CurrentControllerFor != null && elements[i].CurrentControllerFor.Length > 0 && elements[i].CurrentIsDataValidForForm == 0))
                        {
                            return false;
                        }
                    }
                    return true;
                };
        }

        /// <summary>
        /// Func factory for checking that the required child elements are in the accessibility tree.
        /// </summary>
        /// <param name="requiredNames">The names of the elements to search for</param>
        /// <returns>A Func that can be used to verify whether the elements in the list are child elements</returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, bool> CheckChildNames(List<string> requiredNames)
        {
            return (elements, driver, ids) =>
            {
                foreach (var element in elements)
                {
                    var names = element.GetChildNames();
                    if (!requiredNames.All(rn => names.Any(n => n.Contains(rn))))
                    {
                        return false;
                    }
                }
                return true;
            };
        }
    }
}
