using Interop.UIAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Microsoft.Edge.A11y
{
    public class StructureChangedHandler : IUIAutomationStructureChangedEventHandler
    {
        public string ElementName { get; set; }

        void IUIAutomationStructureChangedEventHandler.HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
        {
            if (changeType == StructureChangeType.StructureChangeType_ChildAdded)
            {
                //Console.WriteLine("{0} -Added {1} child", DateTime.Now.Millisecond, sender.CurrentName);
                if (ElementName.Equals(sender.CurrentName, StringComparison.InvariantCultureIgnoreCase))
                {
                    TestData.Ewh.Set();
                }
            }
        }
    }

    /// <summary>
    /// This is where the logic of the tests is stored
    /// </summary>
    class TestData
    {
        public const string ARFAIL = "Failed additional requirement";
        public const string ARPASS = "";
        public const double epsilon = .001;

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
        /// A func that allows extending the tests for specific elements. If an empty string is
        /// returned, the element passes. Otherwise, an explanation of its failure is returned.
        /// </summary>
        public Func<List<IUIAutomationElement>, DriverManager, List<string>, string> _AdditionalRequirement;
        /// <summary>
        /// If not null, this func will be used to test elements to see if they should be
        /// tested (instead of matching _ControlType).
        /// </summary>
        public Func<IUIAutomationElement, bool> _SearchStrategy;

        /// <summary>
        /// Manual reset event waiter used to wait for elements to be added to UIA tree
        /// </summary>
        public static readonly EventWaitHandle Ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// List of elements to wait for to be added to UIA tree before signaling event waiter
        /// </summary>
        public static readonly string[] WaitForElements = { "volume" };

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
            Func<List<IUIAutomationElement>, DriverManager, List<string>, string> additionalRequirement = null,
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
            var uia = new CUIAutomation8();
            var walker = uia.RawViewWalker;

            return new List<TestData>{
                new TestData("article", "Group", "article",
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "aria-label attribute 3",
                        "h1 referenced by aria-labelledby4",
                        "title attribute 5",
                        "aria-label attribute 7"},
                    new List<string>{
                        "h1 referenced by aria-describedby6",
                        "title attribute 7"
                    })),
                new TestData("aside", "Group", "aside", "Custom", "complementary",
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "aria-label attribute 3",
                        "h1 referenced by aria-labelledby4",
                        "title attribute 5",
                        "aria-label attribute 7"},
                    new List<string>{
                        "h1 referenced by aria-describedby6",
                        "title attribute 7"
                    })),
                new TestData("audio", "Group", "audio",
                    additionalRequirement: ((elements, driver, ids) => {
                        var childNames = CheckChildNames(new List<string> {
                                "Play",
                                "Time elapsed",
                                "Seek",
                                "Time remaining",
                                "Mute",
                                "Volume"})//,false, element => element.CurrentIsOffscreen != 1)//TODO enable this if necessary
                                (elements, driver, ids);
                        if(childNames != ARPASS){
                            return childNames;
                        }
                        return CheckAudioKeyboardInteractions(elements, driver, ids);
                    })),
                new TestData("canvas", "Image",
                    additionalRequirement: ((elements, driver, ids) => {
                        var result = string.Empty;

                        var browserElement = EdgeA11yTools.FindBrowserDocument(0);
                        var automationElementConverter = new ElementConverter();

                        HashSet<string> foundControlTypes;
                        elements = EdgeA11yTools.SearchChildren(browserElement, "", (current) => {
                            var convertedRole = automationElementConverter.GetElementNameFromCode(current.CurrentControlType);
                            return convertedRole == "Button" || convertedRole == "Text";
                        }, out foundControlTypes);

                        result += elements.Count() == 2 ? ARPASS : "Unable to find subdom elements";

                        var featureDetectionScript = @"canvas = document.getElementById('myCanvas');
                                                        isSupported = !!(canvas.getContext && canvas.getContext('2d'));
                                                        isSupported = isSupported && !!(canvas.getContext('2d').drawFocusIfNeeded);
                                                        return isSupported;";

                        result += (bool) driver.ExecuteScript(featureDetectionScript, timeout) ? "" : "\nFailed feature detection";

                        return result;
                    })),
                new TestData("datalist", "Combobox", keyboardElements: new List<string> { "input1" },
                    additionalRequirement: ((elements, driver, ids) => {
                        Func<string, string> datalistValue = (id) => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                        var result = string.Empty;

                        foreach(var element in elements)
                        {
                            var elementFive = (IUIAutomationElement5)element;
                            List<int> patternIds;
                            var names = elementFive.GetPatterns(out patternIds);

                            if (!names.Contains("SelectionPattern"))
                            {
                                result += "\nElement did not support SelectionPattern";
                            }
                            else {
                                var selectionPattern = (IUIAutomationSelectionPattern)elementFive.GetCurrentPattern(
                                    patternIds[names.IndexOf("SelectionPattern")]);

                                result += selectionPattern.CurrentCanSelectMultiple == 1 ? "\nCanSelectMultiple set to true" : "";
                            }
                        }
                        var previousControllerForElements = new HashSet<int>();

                        //keyboard a11y
                        foreach (var id in ids)
                        {
                            var initial = datalistValue(id);
                            driver.SendSpecialKeys(id, "Arrow_down");

                            var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().ConvertAll(element => elements.IndexOf(element));
                            if(controllerForElements.All(element => previousControllerForElements.Contains(element))){
                                result += "Element controller for not set for id: " + id;
                            }

                            previousControllerForElements.Add(controllerForElements.First(element => !previousControllerForElements.Contains(element)));

                            driver.SendSpecialKeys(id, "Enter");
                            if (datalistValue(id) != "Item value 1")
                            {
                                return "Unable to set the datalist with keyboard for element with id: " + id;
                            }
                        }

                        return result;
                    })),
                new TestData("details", null),
                new TestData("dialog", null),
                new TestData("figure", "Group", "figure",
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "aria-label attribute 2",
                        "p referenced by aria-labelledby3",
                        "title attribute 4",
                        "Figcaption element 5",
                        "Figcaption element 7"},
                    new List<string>{
                        "p referenced by aria-describedby6",
                        "title attribute 7"
                    })),
                new TestData("figure-figcaption", "",
                    searchStrategy: element => true, //Verify this element via text range
                    additionalRequirement: ((elements, driver, ids) =>
                        {
                            var result = "";

                            var logoText = "HTML5 logo 1";

                            //there will be only one, since element is the pane in this case
                            foreach(var element in elements) {
                                var five = (IUIAutomationElement5)element;
                                List<int> patternIds;
                                var names = five.GetPatterns(out patternIds);

                                if (!names.Contains("TextPattern"))
                                {
                                    return "\nPane did not support TextPattern, unable to search";
                                }

                                var textPattern = (IUIAutomationTextPattern)five.GetCurrentPattern(
                                    patternIds[names.IndexOf("TextPattern")]);

                                var documentRange = textPattern.DocumentRange;

                                var foundText = documentRange.GetText(1000);
                                if (!foundText.Contains(logoText))
                                {
                                    result += "\nText not found on page";
                                }

                                var foundControlTypes = new HashSet<string>();
                                var figure = EdgeA11yTools.SearchChildren(element, "Group", null, out foundControlTypes);

                                var childRange = textPattern.RangeFromChild(figure[0]);

                                var childRangeText = childRange.GetText(1000).Trim();

                                if(childRangeText != logoText)
                                {
                                    result += string.Format("\nUnable to find correct text range. Found '{0}' instead", childRangeText);
                                }
                            }

                            //TOOD not in tree
                            return result;
                        })),
                new TestData("footer", "Group",
                    searchStrategy: element =>
                        element.CurrentControlType == converter.GetElementCodeFromName("Group")
                        && element.CurrentLocalizedControlType == "footer",
                    additionalRequirement: (elements, driver, ids) => {
                        var result = CheckElementNames(
                            new List<string>{
                                "aria-label attribute 3",
                                "small referenced by aria-labelledby4",
                                "title attribute 5",
                                "aria-label attribute 7"},
                            new List<string>{
                                "small referenced by aria-describedby6",
                                "title attribute 7"
                            })(elements, driver, ids);

                        if (result != ARPASS)
                        {
                            return result;
                        }

                        //not all elements need to have the same localized control type
                        if(elements.Select(e => e.CurrentLocalizedControlType).Count(lct => lct == "footer") != 6){
                            return "Elements did not have the correct localized control types";
                        }

                        var elementConverter = new ElementConverter();

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;
                        //same for landmark and localizedlandmark
                        foreach (var element in elements)
                        {
                            var five = element as IUIAutomationElement5;
                            var convertedLandmark = elementConverter.GetElementNameFromCode(five.CurrentLandmarkType);
                            var localizedLandmark = five.CurrentLocalizedLandmarkType;
                            if (convertedLandmark == "Custom")
                            {
                                convertedLandmarks++;
                            }
                            if (localizedLandmark == "content information")
                            {
                                localizedLandmarks++;
                            }
                        }
                        if (convertedLandmarks != 6)
                        {
                            return "Elements did not have the correct landmark types";
                        }

                        if (localizedLandmarks != 6)
                        {
                            return "Elements did not have the correct localized landmark types";
                        }

                        return ARPASS;
                    }),
                new TestData("header", "Group",
                    searchStrategy: element =>
                        element.CurrentControlType == converter.GetElementCodeFromName("Group")
                        && element.CurrentLocalizedControlType == "header",
                    additionalRequirement: (elements, driver, ids) => {
                        var result = CheckElementNames(
                            new List<string>{
                                "aria-label attribute 3",
                                "h1 referenced by aria-labelledby4",
                                "title attribute 5",
                                "aria-label attribute 7"},
                            new List<string>{
                                "h1 referenced by aria-describedby6",
                                "title attribute 7"
                            })(elements, driver, ids);

                        if (result != ARPASS)
                        {
                            return result;
                        }

                        //not all elements need to have the same localized control type
                        if(elements.Select(e => e.CurrentLocalizedControlType).Count(lct => lct == "header") != 6){
                            return "Elements did not have the correct localized control types";
                        }

                        var elementConverter = new ElementConverter();

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;
                        //same for landmark and localizedlandmark
                        foreach (var element in elements)
                        {
                            var five = element as IUIAutomationElement5;
                            var convertedLandmark = elementConverter.GetElementNameFromCode(five.CurrentLandmarkType);
                            var localizedLandmark = five.CurrentLocalizedLandmarkType;
                            if (convertedLandmark == "Custom")
                            {
                                convertedLandmarks++;
                            }
                            if (localizedLandmark == "banner")
                            {
                                localizedLandmarks++;
                            }
                        }
                        if (convertedLandmarks != 6)
                        {
                            return "Elements did not have the correct landmark types";
                        }

                        if (localizedLandmarks != 6)
                        {
                            return "Elements did not have the correct localized landmark types";
                        }

                        return ARPASS;
                    }),
                new TestData("input-color", "Button", "color picker",
                    additionalRequirement: (elements, driver, ids) => {
                        var result = string.Empty;

                        var previousControllerForElements = new HashSet<int>();
                        foreach(var id in ids)
                        {
                            Func<string> CheckColorValue = () => (string) driver.ExecuteScript("return document.getElementById('"+ id + "').value", timeout);
                            var initial = CheckColorValue();

                            //open dialog to check controllerfor
                            driver.SendSpecialKeys(id, "Enter");
                            var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().ConvertAll(element => elements.IndexOf(element));
                            if(controllerForElements.All(element => previousControllerForElements.Contains(element))){
                                result += "Element controller for not set for id: " + id;
                            }
                            driver.SendSpecialKeys(id, "Escape");

                            driver.SendSpecialKeys(id, "EnterTabEscape");
                            if (CheckColorValue() != initial)
                            {
                                result += "\nUnable to cancel with escape";
                            }

                            //TODO open with space as well
                            driver.SendSpecialKeys(id, "EnterTabEnter");
                            if (CheckColorValue() == initial)
                            {
                                result += "\nUnable to submit with enter";
                            }

                            initial = CheckColorValue();

                            driver.SendSpecialKeys(id, "EnterTabTabArrow_rightArrow_rightArrow_rightEnter");
                            if (CheckColorValue() == initial)
                            {
                                result += "\nUnable to change values with arrow keys";
                            }

                            //p1
                            //TODO get to buttons
                            //TODO active buttons with space/enter

                            //p2
                            //TODO enter submits escape cancels
                            //TODO move sliders with left right
                            //TODO children have correct sliders(CT: Slider,
                            //Range.RangeValue) is whatever will be submitted
                            //TODO root button has controllerfor pointing to dialog
                            //TODO ControllerFor and LiveSetting=polite for color well

                        }
                        return result;
                    }),
                new TestData("input-date", "Edit",
                    keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => {
                        return CheckCalendar(3)(elements, driver, ids) + "\n" +
                            CheckElementNames(
                                new List<string>{
                                    "aria-label attribute2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping input 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7"},
                                new List<string>{
                                    "p referenced by aria-describedby6",
                                    "title attribute 7" })
                                (elements, driver, ids);
                    }),
                new TestData("input-datetime-local", "Edit",
                    additionalRequirement: (elements, driver, ids) => {
                        return CheckDatetimeLocal()(elements, driver, ids) + "\n" +
                            CheckElementNames(
                                new List<string>{
                                    "aria-label attribute2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping input 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7"},
                                new List<string>{
                                    "p referenced by aria-describedby6",
                                    "title attribute 7" })
                                (elements, driver, ids);
                    }),
                new TestData("input-email", "Edit", "email",
                    keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => {
                        return CheckValidation()(elements, driver, ids) + "\n" +
                            CheckElementNames(
                                new List<string>{
                                    "aria-label attribute2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping input 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7"},
                                new List<string>{
                                    "p referenced by aria-describedby6",
                                    "title attribute 7" })
                                (elements, driver, ids) +
                                CheckClearButton()(driver, ids);
                    }),
                new TestData("input-month", "Edit", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => {
                        return CheckCalendar(2)(elements, driver, ids) + "\n" +
                            CheckElementNames(
                                new List<string>{
                                    "aria-label attribute2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping input 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7"},
                                new List<string>{
                                    "p referenced by aria-describedby6",
                                    "title attribute 7" })
                                (elements, driver, ids);
                    }),
                new TestData("input-number", "Spinner", "number",
                    keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => {
                        var result = CheckValidation()(elements, driver, ids);
                        result +=
                            CheckElementNames(
                                new List<string>{
                                    "aria-label attribute2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping input 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7"},
                                new List<string>{
                                    "p referenced by aria-describedby6",
                                    "title attribute 7" })
                                (elements, driver, ids);

                        foreach(var id in ids)
                        {
                            //clear all elements
                            driver.ExecuteScript("document.getElementById('" + id + "').value = ''", 0);
                            //put values in to check for later
                            driver.SendKeys(id, "706");
                        }

                        foreach(var element in elements)
                        {
                            var elementFive = (IUIAutomationElement5)element;
                            List<int> patternIds;
                            var names = elementFive.GetPatterns(out patternIds);

                            //TODO reevaluate if RangeValue is actually the right thing
                            if (!names.Contains("RangeValuePattern"))
                            {
                                result += "\nElement did not support RangeValuePattern";
                            }
                            else {
                                var valuePattern = (IUIAutomationRangeValuePattern)elementFive.GetCurrentPattern(
                                    patternIds[names.IndexOf("RangeValuePattern")]);

                                var currentValue = valuePattern.CurrentValue;
                                if(Math.Abs(currentValue - 706) > epsilon)
                                {
                                    result += "\nUnable to retrieve the value with Rangevalue.value";
                                }

                                valuePattern.SetValue(707);
                                currentValue = valuePattern.CurrentValue;
                                if(Math.Abs(currentValue - 707) > epsilon)
                                {
                                    result += "\nUnable to set the value with SetValue()";
                                }
                            }
                        }

                        return result;
                    }),
                new TestData("input-range", "Slider", keyboardElements: new List<string> { "input1", "input2" },
                    //TODO maybe a livesetting:polite, wait for cyns before implementing
                    additionalRequirement: (elements, driver, ids) => {
                        var result = "";

                        //keyboard interaction
                        foreach(var id in ids){
                            Func<int> RangeValue = () => (int) Int32.Parse((string) driver.ExecuteScript("return document.getElementById('" + id + "').value", 0));

                            var initial = RangeValue();
                            driver.SendSpecialKeys(id, "Arrow_up");
                            if (initial == RangeValue())
                            {
                                result += "\nUnable to increase range with arrow up";
                                break;
                            }
                            driver.SendSpecialKeys(id, "Arrow_down");
                            if (initial != RangeValue())
                            {
                                result += "\nUnable to decrease range with arrow down";
                                break;
                            }

                            driver.SendSpecialKeys(id, "Arrow_right");
                            if (initial >= RangeValue())
                            {
                                result += "\nUnable to increase range with arrow right";
                                break;
                            }
                            driver.SendSpecialKeys(id, "Arrow_left");
                            if (initial != RangeValue())
                            {
                                result += "\nUnable to decrease range with arrow left";
                                break;
                            }
                        }

                        //rangevalue pattern
                        foreach(var element in elements){
                            if (!element.GetPatterns().Contains("RangeValuePattern")) {
                                result += "\nElement did not implement the RangeValuePattern";
                            }
                        }

                        //naming
                        result += CheckElementNames(
                            new List<string>
                            {
                                "aria-label attribute 2",
                                "p referenced by aria-labelledby3",
                                "label wrapping input 4",
                                "title attribute 5",
                                "label referenced by for/id attributes 7",
                            },
                            new List<string>
                            {
                                "p referenced by aria-describedby6",
                                "title attribute 7"
                            })(elements, driver, ids);

                        //clear button
                        result += CheckClearButton()(driver, ids);

                        return result;
                    }),
                new TestData("input-search", "Edit", "search", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: CheckElementNames(
                            new List<string>
                            {
                                "aria-label attribute 2",
                                "p referenced by aria-labelledby3",
                                "label wrapping input 4",
                                "title attribute 5",
                                "label referenced by for/id attributes 7",
                            },
                            new List<string>
                            {
                                "p referenced by aria-describedby6",
                                "title attribute 7"
                            })),
                new TestData("input-tel", "Edit", "telephone", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => CheckElementNames(
                            new List<string>
                            {
                                "aria-label attribute 2",
                                "p referenced by aria-labelledby3",
                                "label wrapping input 4",
                                "title attribute 5",
                                "label referenced by for/id attributes 7",
                            },
                            new List<string>
                            {
                                "p referenced by aria-describedby6",
                                "title attribute 7"
                            })(elements, driver, ids) +
                            //clear button
                            CheckClearButton()(driver, ids)
                            ),
                new TestData("input-time", "Edit", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => {
                        return CheckCalendar(2)(elements, driver, ids) + "\n" +
                            CheckElementNames(
                                new List<string>{
                                    "aria-label attribute 2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping input 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7"},
                                new List<string>{
                                    "p referenced by aria-describedby6",
                                    "title attribute 7" })
                                (elements, driver, ids);
                    }),
                new TestData("input-url", "Edit", "url",
                        keyboardElements: new List<string> { "input1", "input2" },
                        additionalRequirement: CheckValidation()),
                new TestData("input-week", "Edit", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => {
                        return CheckCalendar(2)(elements, driver, ids) + "\n" +
                            CheckElementNames(
                                new List<string>{
                                    "aria-label attribute 2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping input 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7"},
                                new List<string>{
                                    "p referenced by aria-describedby6",
                                    "title attribute 7" })
                                (elements, driver, ids);
                    }),
                new TestData("main", "Group", "main", "Main", "main",
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "title attribute 1",
                        "aria-label attribute 2",
                        "h1 referenced by aria-labelledby3",
                        "title attribute 4",
                        "aria-label attribute 6"
                    },
                    new List<string>{
                        "h1 referenced by aria-describedby5",
                        "title attribute 6"
                    })),
                new TestData("mark", "Text", "mark",//TODO text pattern
                    //TODO styleid = Custom, stylename = mark
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "aria-label attribute2",
                        "Element referenced by aria-labelledby attribute3",
                        "title attribute 4",
                        "aria-label attribute 6"
                    },
                    new List<string>{
                        "Element referenced by aria-describedby attribute5",
                        "title attribute 6"
                    })),
                new TestData("meter", "Progressbar", "meter",
                    additionalRequirement:
                        ((elements, driver, ids) => {
                            var result = "";
                            //readonly
                            if(!elements.All(element => element.GetProperties().Any(p => p.Contains("IsReadOnly")))){
                                result += "Not all elements were read only";
                            }

                            //naming
                            result += CheckElementNames(
                                new List<string>
                                {
                                    "aria-label attribute 2",
                                    "p referenced by aria-labelledby3",
                                    "label wrapping meter 4",
                                    "title attribute 5",
                                    "label referenced by for/id attributes 7",
                                },
                                new List<string>
                                {
                                    "p referenced by aria-describedby6",
                                    "title attribute 7"
                                })(elements, driver, ids);

                            //rangevalue
                            foreach (var element in elements)
                            {
                                List<string> patternNames;
                                List<int> patternIds;
                                var patternName = "RangeValuePattern";

                                patternNames = element.GetPatterns(out patternIds);
                                if(!patternNames.Contains(patternName))
                                {
                                    result += "\nElement did not support " + patternName;
                                }
                                else
                                {
                                    IUIAutomationRangeValuePattern rangeValuePattern = ((IUIAutomationElement5)element).GetCurrentPattern(patternIds[patternNames.IndexOf(patternName)]);
                                    if (rangeValuePattern.CurrentMaximum - 100 > epsilon)
                                    {
                                        result += "\nElement did not have the correct max";
                                    }
                                    if (rangeValuePattern.CurrentMinimum - 0 > epsilon)
                                    {
                                        result += "\nElement did not have the correct min";
                                    }
                                    var value = 83.5;//All the meters are set to this
                                    if (rangeValuePattern.CurrentValue - value > epsilon)
                                    {
                                        result += "\nElement did not have the correct value";
                                    }
                                }
                            }

                            //value
                            foreach (var element in elements)
                            {
                                List<string> patternNames;
                                List<int> patternIds;
                                var patternName = "ValuePattern";

                                patternNames = element.GetPatterns(out patternIds);
                                if(!patternNames.Contains(patternName))
                                {
                                    result += "\nElement did not support " + patternName;
                                }
                                else
                                {
                                    IUIAutomationValuePattern valuePattern = ((IUIAutomationElement5)element).GetCurrentPattern(patternIds[patternNames.IndexOf(patternName)]);
                                    var currentValue = valuePattern.CurrentValue;
                                    if (currentValue == null || currentValue == "")
                                    {
                                        result += "\nElement did not have value.value set";
                                        //TODO make sure good/fair/poor is tested
                                    }
                                }
                            }

                            return result;
                        }),
                    searchStrategy: (element => element.GetPatterns().Contains("RangeValuePattern"))),//NB the ControlType is not used for searching this element//TODO why?
                new TestData("menuitem", null),
                new TestData("menupopup", null),
                new TestData("menutoolbar", null),
                new TestData("nav", "Group", "navigation", "Navigation", "navigation",
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "aria-label attribute 2",
                        "h1 referenced by aria-labelledby3",
                        "title attribute 4",
                        "aria-label attribute 6"},
                    new List<string>{
                        "h1 referenced by aria-describedby5",
                        "title attribute 6"
                    })),
                new TestData("output", "Group", "output",
                    additionalRequirement: ((elements, driver, ids) => {
                        var result = string.Empty;

                        if (!elements.All(element => ((IUIAutomationElement5)element).CurrentLiveSetting != LiveSetting.Polite)){
                            result += "Element did not have LiveSetting = Polite";
                        }
                        var controllerForLengths = elements.ConvertAll(element => element.CurrentControllerFor != null ? element.CurrentControllerFor.Length : 0);
                        if (controllerForLengths.Count(cfl => cfl > 0) != 1)
                        {
                            result += "Expected 1 element with ControllerFor set. Found " + controllerForLengths.Count(cfl => cfl > 0);
                        }
                        result += CheckElementNames(
                            new List<string>{
                                "aria-label attribute 2",
                                "p referenced by aria-labelledby3",
                                "label wrapping output 4",
                                "title attribute 5",
                                "label referenced by for/id attributes 7"
                            },
                            new List<string>{
                                "p referenced by aria-describedby6",
                                "title attribute 7"
                            })(elements, driver, ids);

                        return result;
                    })),
                new TestData("progress", "Progressbar",
                    //TODO rangevalue.value .maximum .minimum
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "aria-label attribute 2",
                        "p referenced by aria-labelledby3",
                        "label wrapping output 4",
                        "title attribute 5",
                        "label referenced by for/id attributes 7"
                    },
                    new List<string>{
                        "p referenced by aria-describedby6",
                        "title attribute 7"
                    })),
                new TestData("section", "Group", "section", "Custom", "region",
                    additionalRequirement: CheckElementNames(
                    new List<string>{
                        "aria-label attribute 3",
                        "h1 referenced by aria-labelledby4",
                        "title attribute 5",
                        "aria-label attribute 7"},
                    new List<string>{
                        "h1 referenced by aria-describedby6",
                        "title attribute 7"
                    })),
                new TestData("summary", null),
                new TestData("time", "Group", "time",
                    additionalRequirement: ((elements, driver, ids) => {
                        if (!elements.All(element => {
                            var fullDescription = ((IUIAutomationElement6)element).CurrentFullDescription;
                            return fullDescription != null && fullDescription == "2015-10-28";}))
                        {
                            return "Element did not have the correct FullDescription";
                        }
                        return ARPASS;
                    })),
                new TestData("track", "track",
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        driver.ExecuteScript(Javascript.Track, timeout);

                        return (bool)driver.ExecuteScript("return Modernizr.track && Modernizr.texttrackapi", timeout) ? ARPASS :
                            "Element was not found to be supported by Modernizr";
                    }),
                    searchStrategy: (element => true)),
                new TestData("video", "Group", null, keyboardElements: new List<string> { "video1" },
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        var childNames = CheckChildNames(
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
                                    "Full screen" })(elements, driver, ids);
                        if(childNames != ARPASS){
                            return childNames;
                        }
                        return ARPASS;//CheckVideoKeyboardInteractions(elements, driver, ids);
                    })),
                    new TestData("hidden-att", "Button", null,
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        var result = string.Empty;

                        var elementConverter = new ElementConverter();
                        var paneCode = elementConverter.GetElementCodeFromName("Pane");

                        var browserElement = EdgeA11yTools.FindBrowserDocument(0);

                        if (elements.Count(e => e.CurrentControlType != paneCode) != 0)
                        {
                            return "Found " + elements.Count(e => e.CurrentControlType != paneCode) + " elements. Expected 0";
                        }

                        //Make sure the text isn't showing up on the page
                        var five = (IUIAutomationElement5)elements[0];//only have the pane element
                        List<int> patternIds;
                        var names = five.GetPatterns(out patternIds);

                        if (!names.Contains("TextPattern"))
                        {
                            return "Pane did not support TextPattern, unable to search";
                        }

                        var textPattern = (IUIAutomationTextPattern)five.GetCurrentPattern(
                            patternIds[names.IndexOf("TextPattern")]);

                        var documentRange = textPattern.DocumentRange;

                        var foundText = documentRange.GetText(1000);
                        if(foundText.Contains("HiDdEn TeXt"))
                        {
                            result += "\nFound text that should have been hidden";
                        }

                        //remove hidden attribute
                        driver.ExecuteScript(Javascript.RemoveHidden, timeout);

                        //make sure the buttons show up now that their parents are not hidden
                        HashSet<string> foundControlTypes;
                        elements = EdgeA11yTools.SearchChildren(browserElement, "Button", null, out foundControlTypes);
                        if (elements.Count(e => e.CurrentControlType != paneCode) != 2)
                        {
                            result += "\nFound " + elements.Count(e => e.CurrentControlType != paneCode) + " elements. Expected 2";
                        }

                        return result;
                    }),
                    searchStrategy: (element => true)),//take the pane
                new TestData("required-att", "Edit",
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        driver.SendSubmit("input1");
                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        //TODO check all as with CheckValidation
                        foreach(var element in elements){
                            if(element.CurrentControllerFor == null || element.CurrentControllerFor.Length == 0){
                                return "\nElement did not have controller for set";
                            }

                            if(element.CurrentIsDataValidForForm != 0){
                                return "\nElement did not have IsDataValidForForm set to false";
                            }

                            if(element.CurrentIsRequiredForForm != 1){
                                return "\nElement did not have IsRequiredForForm set to true";
                            }

                            if(element.CurrentHelpText == null || element.CurrentHelpText.Length == 0){
                                return "\nElement did not have HelpText";
                            }
                        }

                        return ARPASS;
                    }),
                new TestData("placeholder-att", "Edit",
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        if (elements.Count() != 6)
                        {
                            return ARFAIL;
                        }

                        var elementNames = elements.Select(element => element.CurrentName).ToList();
                        return new List<Func<List<string>, bool>>{
                            names => names.Contains("placeholder text 1"),
                            names => names.Contains("Label text 2:"),
                            names => names.Contains("Label text 3:"),
                            names => names.Contains("placeholder text 4"),
                            names => names.Contains("placeholder text 5"),
                            names => names.Contains("aria-placeholder text 6"),
                        }.All(f => f(elementNames)) ? ARPASS : ARFAIL;
                    }))
            };
        }

        /// <summary>
        /// Check basic keyboard interactions for the video control
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="driver"></param>
        /// <param name="ids"></param>
        /// <returns>An empty string if an element fails, otherwise an explanation</returns>
        private static string CheckVideoKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids)
        {
            string videoId = "video1";
            string result = ARPASS;

            Func<bool> VideoPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + videoId + "').paused", 0);
            Func<object> PauseVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').pause()", 0);
            Func<object> PlayVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').play()", 0);
            Func<double> GetVideoVolume = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').volume", 0).ParseMystery();
            Func<double, bool> VideoVolume = expected => Math.Abs(GetVideoVolume() - expected) < epsilon;
            Func<bool> VideoMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').muted", 0);
            Func<double> GetVideoElapsed = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').currentTime", 0).ParseMystery();
            Func<double, bool> VideoElapsed = expected => Math.Abs(GetVideoElapsed() - expected) < epsilon;
            Func<bool> IsVideoFullScreen = () => driver.ExecuteScript("return document.fullscreenElement", 0) != null;

            var handler = new StructureChangedHandler();
            WaitForElement(handler, elements[0], "volume");

            //Case 1: tab to play button and play/pause
            Console.WriteLine("Case 1: tab to play button and play/pause");
            driver.SendSpecialKeys(videoId, "TabSpace");

            WaitForElement(handler, elements[0], "play");
            if (!WaitForCondition(VideoPlaying))
            {
                result += "\tVideo was not playing after spacebar on play button\n";
                PlayVideo();
            }
            driver.SendSpecialKeys(videoId, "Enter");
            if (WaitForCondition(VideoPlaying))
            {
                result += "\tVideo was not paused after enter on play button\n";
                PauseVideo();
            }

            //Case 2: Volume and mute
            Console.WriteLine("Case 2: Volume and mute");
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 6);//tab to volume control//TODO make this more resilient to UI changes
            driver.SendSpecialKeys(videoId, "Enter");//mute
            if (!WaitForCondition(VideoMuted))
            {
                result += "\tEnter did not mute the video\n";
            }
            WaitForElement(handler, elements[0], "mute");
            driver.SendSpecialKeys(videoId, "Enter");//unmute
            if (WaitForCondition(VideoMuted))
            {
                result += "\tEnter did not unmute the video\n";
            }
            var initial = GetVideoVolume();
            driver.SendSpecialKeys(videoId, "Arrow_downArrow_down");//volume down
            if (!WaitForCondition(VideoVolume, initial - 0.1))
            {
                result += "\tVolume did not decrease with arrow keys\n";
            }
            driver.SendSpecialKeys(videoId, "Arrow_upArrow_up");//volume up
            if (!WaitForCondition(VideoVolume, initial))
            {
                result += "\tVolume did not increase with arrow keys\n";
            }

            //Case 3: Audio selection
            // TODO test manually
            //Javascript.ClearFocus(driver, 0);
            //driver.SendTabs(videoId, 5);//tab to audio selection
            //driver.SendSpecialKeys(videoId, "EnterArrow_down");

            Console.WriteLine("Case 4: Progress and seek");
            //Case 4: Progress and seek
            if (WaitForCondition(VideoPlaying))
            { //this should not be playing
                result += "\tVideo was playing when it shouldn't have been\n";
            }
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 3);//tab to seek
            initial = GetVideoElapsed();
            driver.SendSpecialKeys(videoId, "Arrow_right"); //skip ahead
            if (!WaitForCondition(VideoElapsed, initial + 10))
            {
                result += "\tVideo did not skip forward with arrow right\n";
            }

            driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
            if (!WaitForCondition(VideoElapsed, initial))
            {
                result += "\tVideo did not skip back with arrow left\n";
                Console.WriteLine("Video did not skip back with arrow left, initial:{0} actual:{1}", initial, GetVideoElapsed());
            }

            //Case 5: Progress and seek on remaining time
            if (VideoPlaying())
            { //this should not be playing
                result += "\tVideo was playing when it shouldn't have been\n";
            }
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 4);//tab to seek
            initial = GetVideoElapsed();
            driver.SendSpecialKeys(videoId, "Arrow_right"); //skip ahead
            if (!WaitForCondition(VideoElapsed, initial + 10))
            {
                result += "\tVideo did not skip forward with arrow right\n";
            }

            driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
            if (!WaitForCondition(VideoElapsed, initial))
            {
                result += "\tVideo did not skip back with arrow left\n";
                Console.WriteLine("Video did not skip back with arrow left, initial:{0} actual:{1}", initial, GetVideoElapsed());
                driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
                Console.WriteLine("Video did not skip back with arrow left, initial:{0} actual:{1}", initial, GetVideoElapsed());
            }

            //Case 6: Full screen
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 8);//tab to fullscreen
            driver.SendSpecialKeys(videoId, "Enter"); //enter fullscreen mode
            if (!WaitForCondition(IsVideoFullScreen))
            {
                result += "\tVideo did not enter FullScreen mode\n";
            }
            driver.SendSpecialKeys(videoId, "Escape");
            if (WaitForCondition(IsVideoFullScreen))
            {
                result += "\tVideo did not exit FullScreen mode\n";
            }

            return result;
        }

        /// <summary>
        /// Check basic keyboard interactions for the audio control
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="driver"></param>
        /// <param name="ids"></param>
        /// <returns>An empty string if an element fails, otherwise an explanation</returns>
        private static string CheckAudioKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids)
        {
            string audioId = "audio1";
            string result = ARPASS;
            Func<bool> AudioPlaying = () =>
            {
                Thread.Sleep(500);
                return (bool)driver.ExecuteScript("return !document.getElementById('" + audioId + "').paused", 0);
            };
            Func<object> PauseAudio = () =>
            {
                Thread.Sleep(500);
                return driver.ExecuteScript("!document.getElementById('" + audioId + "').pause()", 0);
            };
            Func<object> PlayAudio = () =>
            {
                Thread.Sleep(500);
                return driver.ExecuteScript("!document.getElementById('" + audioId + "').play()", 0);
            };
            Func<double> AudioVolume = () =>
            {
                Thread.Sleep(500);
                return driver.ExecuteScript("return document.getElementById('" + audioId + "').volume", 0).ParseMystery();
            };
            Func<bool> AudioMuted = () =>
            {
                Thread.Sleep(500);
                return (bool)driver.ExecuteScript("return document.getElementById('" + audioId + "').muted", 0);
            };
            Func<double> AudioElapsed = () =>
            {
                Thread.Sleep(500);
                return driver.ExecuteScript("return document.getElementById('" + audioId + "').currentTime", 0).ParseMystery();
            };

            WaitForElement(elements[0], "volume");

            //Case 1: Play/Pause
            driver.SendTabs(audioId, 1); //Tab to play button
            driver.SendSpecialKeys(audioId, "Enter");
            if (!AudioPlaying())
            {
                result += "\tAudio did not play with enter\n";
                PlayAudio();
            }

            driver.SendSpecialKeys(audioId, "Space");
            if (AudioPlaying())
            {
                result += "\tAudio did not pause with space\n";
                PauseAudio();
            }

            //Case 2: Seek
            if (AudioPlaying())
            {
                result += "\tAudio was playing when it shouldn't have been\n";
            }
            driver.SendTabs(audioId, 3);
            var initial = AudioElapsed();
            driver.SendSpecialKeys(audioId, "Arrow_right");
            if (initial == AudioElapsed())
            {
                result += "\tAudio did not skip forward with arrow right\n";
            }
            driver.SendSpecialKeys(audioId, "Arrow_left");
            if (initial != AudioElapsed())
            {
                result += "\tAudio did not skip back with arrow left\n";
            }

            //Case 3: Volume and mute
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(audioId, 5);
            initial = AudioVolume();
            driver.SendSpecialKeys(audioId, "Arrow_down");
            if (initial == AudioVolume())
            {
                result += "\tVolume did not decrease with arrow down\n";
            }

            driver.SendSpecialKeys(audioId, "Arrow_up");
            if (initial != AudioVolume())
            {
                result += "\tVolume did not increase with arrow up\n";
            }

            driver.SendSpecialKeys(audioId, "Enter");
            if (!AudioMuted())
            {
                result += "\tAudio was not muted by enter on the volume control\n";
            }
            WaitForElement(elements[0], "mute");
            driver.SendSpecialKeys(audioId, "Enter");
            if (AudioMuted())
            {
                result += "\tAudio was not unmuted by enter on the volume control\n";
            }

            return result;
        }

        /// <summary>
        /// Test all date/time elements except for datetime-local, which is tested by the
        /// amended method below.
        /// </summary>
        /// <param name="fields">A count of the number of fields to test</param>
        /// <returns></returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, string> CheckCalendar(int fields)
        {
            //TODO tab to buttons
            return new Func<List<IUIAutomationElement>, DriverManager, List<string>, string>((elements, driver, ids) =>
            {
                var result = "";
                var previousControllerForElements = new HashSet<int>();
                foreach (var id in ids)
                {
                    driver.SendSpecialKeys(id, "EnterEscapeEnterEnter");//Make sure that the element has focus (gets around weirdness in WebDriver)

                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);

                    var today = DateValue();

                    //Open the menu
                    driver.SendSpecialKeys(id, "Enter");

                    //Check ControllerFor
                    var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().ConvertAll(element => elements.IndexOf(element));
                    if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                    {
                        result += "\nElement controller for not set for id: " + id;
                    }

                    //Change each field in the calendar
                    for (int i = 0; i < fields; i++)
                    {
                        driver.SendSpecialKeys(id, "Arrow_downTab");
                    }
                    //Close the menu (only necessary for time)
                    driver.SendSpecialKeys(id, "Enter");

                    //Get the altered value, which should be one off the default
                    //for each field
                    var newdate = DateValue();
                    var newdatesplit = newdate.Split('-', ':');
                    var todaysplit = today.Split('-', ':');

                    //ensure that all fields have been changed
                    for (int i = 0; i < fields; i++)
                    {
                        if (newdatesplit[i] == todaysplit[i])
                        {
                            result += "\nNot all fields were changed by keyboard interaction.";
                        }
                    }
                    //TODO escape to cancel
                    //TODO buttons with space or enter
                    //TODO open with space
                }
                return result;
            });
        }

        /// <summary>
        /// Test the datetime-local input element with an amended version of the method
        /// above.
        /// </summary>
        /// <returns></returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, string> CheckDatetimeLocal()
        {
            return new Func<List<IUIAutomationElement>, DriverManager, List<string>, string>((elements, driver, ids) =>
            {
                var result = "";
                var foundControllerFor = new List<int>();
                foreach (var id in ids)
                {
                    driver.SendSpecialKeys(id, "EnterEscapeEnterEnter");//Make sure that the element has focus (gets around weirdness in WebDriver)

                    var inputFields = new List<int> { 3, 3 };
                    var outputFields = 5;
                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);

                    driver.SendSpecialKeys(id, "EnterEnterTabEnterEnter");

                    var today = DateValue();

                    driver.SendSpecialKeys(id, "ShiftTabShift");//Shift focus back to main control
                    foreach (var count in inputFields)
                    {
                        //Open the menu
                        driver.SendSpecialKeys(id, "Enter");

                        //Check ControllerFor
                        var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().ConvertAll(element => elements.IndexOf(element));
                        if (controllerForElements.Count() == 0)
                        {
                            result += "\nElement controller for not set for id: " + id;
                        }
                        else
                        {
                            foundControllerFor = foundControllerFor.Union(controllerForElements).ToList();
                        }

                        //Change each field in the calendar
                        for (int i = 0; i < count; i++)
                        {
                            driver.SendSpecialKeys(id, "Arrow_downTab");
                        }
                    }

                    //Get the altered value, which should be one off the default
                    //for each field
                    var newdate = DateValue();
                    var newdatesplit = newdate.Split('-', 'T', ':');
                    var todaysplit = today.Split('-', 'T', ':');

                    //ensure that all fields have been changed
                    for (int i = 0; i < outputFields; i++)
                    {
                        if (newdatesplit[i] == todaysplit[i])
                        {
                            result += "\nNot all fields were changed by keyboard interaction";
                        }
                    }
                }

                foreach (var element in elements)
                {
                    //Since we expect each element to be set as the controllerfor twice
                    if (foundControllerFor.Count(fcf => fcf == elements.IndexOf(element)) != 2)
                    {
                        result += "\nElement controller for not set";
                    }
                }
                return result;
            });
        }

        /// <summary>
        /// Func factory for checking that when invalid input is entered into a form,
        /// an error message appears.
        /// </summary>
        /// <returns></returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, string> CheckValidation()
        {
            return (elements, driver, ids) =>
                {
                    //The indices of the elements that have been found to be invalid before
                    var previouslyInvalid = new HashSet<int>();
                    for (var i = 0; i < elements.Count; i++)
                    {
                        driver.SendKeys("input" + (i + 1), "invalid");
                        driver.SendSubmit("input" + (i + 1));
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));

                        //Everything that is invalid on the page
                        var invalid = elements.Where(e => e.CurrentControllerFor != null &&
                                        e.CurrentControllerFor.Length > 0 &&
                                        e.CurrentIsDataValidForForm == 0 &&
                                        e.CurrentHelpText != null &&
                                        e.CurrentHelpText.Length > 0).Select(e => elements.IndexOf(e));

                        //Elements that are invalid for the first time
                        var newInvalid = invalid.DefaultIfEmpty(-1).FirstOrDefault(inv => !previouslyInvalid.Contains(inv));
                        if (newInvalid == -1)
                        {
                            return "Element failed to validate improper input";
                        }

                        if (elements[newInvalid].CurrentIsDataValidForForm != 0)
                        {
                            return "\nElement did not have IsDataValidForForm set to false";
                        }

                        if (elements[newInvalid].CurrentHelpText == null || elements[newInvalid].CurrentHelpText.Length == 0)
                        {
                            return "\nElement did not have HelpText";
                        }

                        if (elements[newInvalid].CurrentControllerFor.Length > 1)
                        {
                            throw new Exception("Test assumption failed: expected only one controller");
                        }

                        var helpPane = elements[newInvalid].CurrentControllerFor.GetElement(0);
                        if (helpPane.CurrentControlType != new ElementConverter().GetElementCodeFromName("Pane"))
                        {
                            return "Error message did not have correct ControlType";
                        }
                        //TODO find out if the message pane needs to have any more requirements

                        previouslyInvalid.Add(newInvalid);
                    }
                    return ARPASS;
                };
        }

        /// <summary>
        /// Func factory for checking that the required child elements are in the accessibility tree.
        /// </summary>
        /// <param name="requiredNames">The names of the elements to search for</param>
        /// <returns>A Func that can be used to verify whether the elements in the list are child elements</returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, string> CheckChildNames(List<string> requiredNames,
            bool strict = false,
            Func<IUIAutomationElement, bool> searchStrategy = null)
        {
            return (elements, driver, ids) =>
            {
                foreach (var element in elements)
                {
                    var names = element.GetChildNames(searchStrategy);
                    if(strict && names.Count() != requiredNames.Count)
                    {
                        return "Found incorrect number of children";
                    }
                    var firstFail = requiredNames.FirstOrDefault(rn => !names.Any(n => n.Contains(rn)));
                    if (firstFail != null)
                    {
                        return "Failed to find " + firstFail;
                    }
                }
                return ARPASS;
            };
        }

        /// <summary>
        /// Func factory for checking that elements have the proper Names and FullDescriptions
        /// </summary>
        /// <param name="requiredNames">All the names we expect to find</param>
        /// <param name="requiredDescriptions">All the descriptions we expect
        /// to find</param>
        /// <returns>A func that can be used to check the names and descriptions
        /// of elements</returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, string> CheckElementNames(List<string> requiredNames, List<string> requiredDescriptions)
        {
            return (elements, driver, ids) =>
            {
                var names = elements.ConvertAll(element => element.CurrentName).Where(e => !string.IsNullOrEmpty(e)).ToList();
                var descriptions = elements.ConvertAll(element => ((IUIAutomationElement6)element).CurrentFullDescription).Where(e => !string.IsNullOrEmpty(e)).ToList();
                var result = "";

                //Check names
                var expectedNotFound = requiredNames.Where(rn => !names.Contains(rn)).ToList();//get a list of all required names not found
                var foundNotExpected = names.Where(n => !requiredNames.Contains(n)).ToList();//get a list of all found names that weren't required
                result +=
                    expectedNotFound.Any() ? "\n" +
                        expectedNotFound.Aggregate((a, b) => a + ", " + b) +
                        (expectedNotFound.Count() > 1 ?
                            " were expected as names but not found. " :
                            " was expected as a name but not found. ")
                        : "";
                result +=
                    foundNotExpected.Any() ? "\n" +
                        foundNotExpected.Aggregate((a, b) => a + ", " + b) +
                        (foundNotExpected.Count() > 1 ?
                            " were found as names but not expected. " :
                            " was found as a name but not expected. ")
                        : "";

                //Check descriptions
                expectedNotFound = requiredDescriptions.Where(rd => !descriptions.Contains(rd)).ToList();
                foundNotExpected = descriptions.Where(d => !requiredDescriptions.Contains(d)).ToList();
                result +=
                    expectedNotFound.Any() ? "\n" +
                        expectedNotFound.Aggregate((a, b) => a + ", " + b) +
                        (expectedNotFound.Count() > 1 ?
                            " were expected as names but not found. " :
                            " was expected as a name but not found. ")
                        : "";
                result +=
                    foundNotExpected.Any() ? "\n" +
                        foundNotExpected.Aggregate((a, b) => a + ", " + b) +
                        (foundNotExpected.Count() > 1 ?
                            " were found as names but not expected. " :
                            " was found as a name but not expected. ")
                        : "";

                return result;
            };
        }

        public static Func<DriverManager, List<string>, string> CheckClearButton()
        {
            return (driver, ids) =>
            {
                var result = "";
                Func<string, string> inputValue = (id) => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                Action<string> clearInput = (id) => driver.ExecuteScript("document.getElementById('" + id + "').value = ''", 0);

                foreach (var id in ids)
                {
                    //Enter something, tab to the clear button, clear with space
                    driver.SendSpecialKeys(id, "xTabSpace");
                    if (inputValue(id) != "")
                    {
                        result += "\nElement did not clear or the clear button was not reachable by tab";
                    }
                    //Don't leave input which could cause problems with other tests
                    clearInput(id);
                }

                return result;
            };
        }

        public static bool WaitForElement(IUIAutomationElement element, string elementName, TimeSpan? timeout = null)
        {
            var handler = new StructureChangedHandler();
            handler.ElementName = elementName;
            new CUIAutomation8().AddStructureChangedEventHandler(element, TreeScope.TreeScope_Descendants, null, handler);
            if (timeout == null)
            {
                timeout = TimeSpan.FromMilliseconds(500);
            }
            return Ewh.WaitOne(timeout.Value);
        }

        public static bool WaitForElement(StructureChangedHandler handler, IUIAutomationElement element, string elementName, TimeSpan? timeout = null)
        {
            handler.ElementName = elementName;
            new CUIAutomation8().AddStructureChangedEventHandler(element, TreeScope.TreeScope_Descendants, null, handler);
            if (timeout == null)
            {
                timeout = TimeSpan.FromMilliseconds(500);
            }
            return Ewh.WaitOne(timeout.Value);
        }

        public static bool WaitForCondition(Func<double, bool> conditionCheck, double value)
        {
            var condition = false;
            for (var i = 0; i < 10 && !condition; i++)
            {
                Thread.Sleep(500);
                condition = conditionCheck(value);
            }
            return condition;
        }

        public static bool WaitForCondition(Func<bool> conditionCheck)
        {
            var condition = false;
            for (var i = 0; i < 10 && !condition; i++)
            {
                Thread.Sleep(500);
                condition = conditionCheck();
            }
            return condition;
        }
    }
}
