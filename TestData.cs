using Interop.UIAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Edge.A11y
{
    /// <summary>
    /// This is used to wait for structure changed events
    /// </summary>
    public class StructureChangedHandler : IUIAutomationStructureChangedEventHandler
    {
        /// <summary>
        /// The element whose structure will be changed
        /// </summary>
        public string ElementName { get; set; }

        /// <summary>
        /// This is called when the event fires
        /// </summary>
        /// <param name="sender">The element in question</param>
        /// <param name="changeType">The type of change</param>
        void IUIAutomationStructureChangedEventHandler.HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
        {
            if (changeType == StructureChangeType.StructureChangeType_ChildAdded)
            {
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
        /// If not null, this func will be used to test elements to see if they should be
        /// tested (instead of matching _ControlType).
        /// </summary>
        public Func<IUIAutomationElement, bool> _SearchStrategy;
        /// <summary>
        /// A list of expected values which will be compared to the accessible names of
        /// the found elements.
        /// </summary>
        public List<string> _requiredNames;
        /// <summary>
        /// Same as above, but for accessible descriptions.
        /// </summary>
        public List<string> _requiredDescriptions;
        /// <summary>
        /// A func that allows extending the tests for specific elements. If an empty string is
        /// returned, the element passes. Otherwise, an explanation of its failure is returned.
        /// </summary>
        public Func<List<IUIAutomationElement>, DriverManager, List<string>, string> _AdditionalRequirement;

        /// <summary>
        /// Manual reset event waiter used to wait for elements to be added to UIA tree
        /// </summary>
        public static readonly EventWaitHandle Ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// List of elements to wait for to be added to UIA tree before signaling event waiter
        /// </summary>
        public static readonly string[] WaitForElements = { "volume" };

        /// <summary>
        /// Simple Ctor
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="controlType"></param>
        /// <param name="localizedControlType"></param>
        /// <param name="landmarkType"></param>
        /// <param name="localizedLandmarkType"></param>
        /// <param name="keyboardElements"></param>
        /// <param name="searchStrategy"></param>
        /// <param name="requiredNames"></param>
        /// <param name="requiredDescriptions"></param>
        /// <param name="additionalRequirement"></param>
        public TestData(string testName,
            string controlType,
            string localizedControlType = null,
            string landmarkType = null,
            string localizedLandmarkType = null,
            List<string> keyboardElements = null,
            Func<IUIAutomationElement, bool> searchStrategy = null,
            List<string> requiredNames = null,
            List<string> requiredDescriptions = null,
            Func<List<IUIAutomationElement>, DriverManager, List<string>, string> additionalRequirement = null)
        {
            _TestName = testName;
            _ControlType = controlType;
            _LocalizedControlType = localizedControlType;
            _LandmarkType = landmarkType;
            _LocalizedLandmarkType = localizedLandmarkType;
            _KeyboardElements = keyboardElements;
            _SearchStrategy = searchStrategy;
            _requiredNames = requiredNames;
            _requiredDescriptions = requiredDescriptions;
            _AdditionalRequirement = additionalRequirement;
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
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"},
                    requiredDescriptions:
                        new List<string>{
                            "h1 referenced by aria-describedby6",
                            "title attribute 7"
                    }),
                new TestData("aside", "Group", "aside", "Custom", "complementary",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"},
                    requiredDescriptions:
                        new List<string>{
                            "h1 referenced by aria-describedby6",
                            "title attribute 7"
                    }),
                new TestData("audio", "Group", "audio",
                    additionalRequirement: ((elements, driver, ids) =>
                        CheckChildNames(new List<string> {
                                "Play",
                                "Time elapsed",
                                "Seek",
                                "Time remaining",
                                "Mute",
                                "Volume"})(elements, driver, ids) +
                        CheckAudioKeyboardInteractions(elements, driver, ids)
                    )),
                new TestData("canvas", "Image",
                    additionalRequirement: ((elements, driver, ids) => {
                        var result = string.Empty;

                        var browserElement = EdgeA11yTools.FindBrowserDocument(0);
                        var automationElementConverter = new ElementConverter();

                        HashSet<string> foundControlTypes;
                        var subdomElements = EdgeA11yTools.SearchChildren(browserElement, "", (current) => {
                            var convertedRole = automationElementConverter.GetElementNameFromCode(current.CurrentControlType);
                            return convertedRole == "Button" || convertedRole == "Text";
                        }, out foundControlTypes);

                        result += subdomElements.Count() == 2 ? "" : "Unable to find subdom elements";

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
                        foreach (var id in ids.Take(1))
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
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "title attribute 4",
                            "Figcaption element 5",
                            "Figcaption element 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                    }),
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
                        && element.CurrentLocalizedControlType != "article",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 3",
                            "small referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"},
                    requiredDescriptions:
                        new List<string>{
                            "small referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement: (elements, driver, ids) => {
                        var result = "";

                        if (elements.Count() != 7)
                        {
                            result += "\nFound " + elements.Count() + " elements, expected 7.";
                        }

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;
                        //same for landmark and localizedlandmark
                        foreach (var element in elements)
                        {
                            var five = element as IUIAutomationElement5;
                            var convertedLandmark = converter.GetElementNameFromCode(five.CurrentLandmarkType);
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
                        if (convertedLandmarks != 1)
                        {
                            result += "\nFound " + convertedLandmarks + " elements with landmark type Custom, expected 1";
                        }

                        if (localizedLandmarks != 1)
                        {
                            result += "\nFound " + localizedLandmarks + " elements with localized landmark type content information, expected 1";
                        }

                        return result;
                    }),
                new TestData("header", "Group",
                    searchStrategy: element =>
                        element.CurrentControlType == converter.GetElementCodeFromName("Group")
                        && element.CurrentLocalizedControlType != "article",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"},
                    requiredDescriptions:
                        new List<string>{
                            "h1 referenced by aria-describedby6",
                            "title attribute 7"},
                    additionalRequirement: (elements, driver, ids) => {
                        var result = "";

                        if (elements.Count() != 7)
                        {
                            result += "\nFound " + elements.Count() + " elements, expected 7.";
                        }

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;
                        var landmarkCode = 0;
                        //same for landmark and localizedlandmark
                        foreach (var element in elements)
                        {
                            var five = element as IUIAutomationElement5;
                            var convertedLandmark = converter.GetElementNameFromCode(five.CurrentLandmarkType);
                            landmarkCode = five.CurrentLandmarkType;
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
                        if (convertedLandmarks != 1)
                        {
                            result += "\nFound " + convertedLandmarks + " elements with landmark type Custom, expected 1: " + landmarkCode;
                        }

                        if (localizedLandmarks != 1)
                        {
                            result += "\nFound " + localizedLandmarks + " elements with localized landmark type banner, expected 1";
                        }

                        return result;
                    }),
                new TestData("input-color", "Button", "color picker",
                    additionalRequirement: (elements, driver, ids) => {
                        var result = string.Empty;

                        var previousControllerForElements = new HashSet<int>();
                        foreach(var id in ids.Take(1))
                        {
                            Func<string> CheckColorValue = () => (string) driver.ExecuteScript("return document.getElementById('"+ id + "').value", timeout);
                            Action<string> ChangeColorValue = (value) => driver.ExecuteScript("document.getElementById('"+ id + "').value = '" + value + "'", timeout);
                            Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                            var initial = CheckColorValue();
                            driver.SendSpecialKeys(id, "EnterEscapeEnterEnter");

                            //open dialog to check controllerfor
                            driver.SendSpecialKeys(id, "Enter");
                            var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().ConvertAll(element => elements.IndexOf(element));
                            if(controllerForElements.All(element => previousControllerForElements.Contains(element))){
                                result += "\nElement controller for not set for id: " + id;
                            }
                            else
                            {
                                //the element that corresponds to this id
                                var thisElement = elements[controllerForElements.First(element => !previousControllerForElements.Contains(element))];
                                if(thisElement.CurrentControllerFor.Length > 1){
                                    throw new Exception("\nMore than one ControllerFor present, test assumption failed");
                                }
                                var thisDialog = thisElement.CurrentControllerFor.GetElement(0);
                                var descendents = thisDialog.GetAllDescendents();

                                //sliders
                                var sliders = descendents.Where(d => d.CurrentControlType == converter.GetElementCodeFromName("Slider"));
                                if(sliders.Count() != 3){
                                    result += "\nDialog did not have three slider elements";
                                }
                                else if (!sliders.All(s => s.GetPatterns().Contains("RangeValuePattern")))
                                {
                                    result += "\nDialog's sliders did not implement RangeValuePattern";
                                }

                                //buttons
                                if (descendents.Count(d => d.CurrentControlType == converter.GetElementCodeFromName("Button")) != 2)
                                {
                                    result += "\nDialog did not have two button elements";
                                }

                                //color well
                                var outputs = descendents.Where(d => converter.GetElementNameFromCode(d.CurrentControlType) == "Group" && d.CurrentLocalizedControlType == "output");
                                if (outputs.Count() > 1)
                                {
                                    throw new Exception("Test assumption failed: expected color dialog to have at most one output");
                                }
                                else if (outputs.Count() == 0)
                                {
                                    result += "\nCould not find output in color dialog";
                                }
                                else if (outputs.Count() == 1)
                                {
                                    var output = outputs.First();
                                    if (output.CurrentName == null || output.CurrentName == "")
                                    {
                                        result += "\nColor dialog output did not have name set";
                                    }
                                    else
                                    {
                                        var initialName = output.CurrentName;

                                        driver.SendSpecialKeys(id, "TabTabArrow_rightArrow_right");

                                        if (output.CurrentName == initialName)
                                        {
                                            result += "\nColor dialog output did not change name when color changed: " + output.CurrentName;
                                        }

                                        ChangeColorValue("#000000");//Ensure that the color is clear before continuing
                                    }
                                }
                            }

                            //open with enter, close with escape
                            driver.SendSpecialKeys(id, "EscapeEnterTabArrow_rightArrow_rightEscape");
                            if (CheckColorValue() != initial)
                            {
                                result += "\nUnable to cancel with escape";
                            }

                            //open with enter, close with enter
                            driver.SendSpecialKeys(id, "EscapeEnterTabTabArrow_rightArrow_rightEnter");
                            if (CheckColorValue() == initial)
                            {
                                result += "\nUnable to change value with arrow keys and submit with enter";
                            }

                            //open with space, close with enter
                            initial = CheckColorValue();
                            driver.SendSpecialKeys(id, "EscapeSpaceTabTabArrow_rightArrow_rightEnter");
                            if (initial == CheckColorValue())
                            {
                                result += "\nUnable to open dialog with space";
                            }

                            initial = CheckColorValue();

                            driver.SendSpecialKeys(id, "EnterTabTabTab");
                            if (ActiveElement() != id)
                            {
                                result += "\nUnable to reach accept/dismiss buttons via tab";
                            }
                            else//only try to use the buttons if they're there
                            {
                                //**Dismiss button**
                                //Open the dialog, change hue, tab to cancel button, activate it with space,
                                //check that tabbing moves to the previous button (on the page not the dialog)
                                driver.SendSpecialKeys(id, "EscapeEnterArrow_rightArrow_rightTabTabTabTabSpaceShiftTabShift");
                                if (initial != CheckColorValue() || ActiveElement() == id)
                                {
                                    result += "\nUnable to cancel with dismiss button via space";
                                }

                                //do the same as above, but activate the button with enter this time
                                driver.SendSpecialKeys(id, "EscapeEnterArrow_rightArrow_rightTabTabTabTabEnterShiftTabShift");
                                if (initial != CheckColorValue() || ActiveElement() == id)
                                {
                                    result += "\nUnable to cancel with dismiss button via enter";
                                }


                                //**Accept button**
                                initial = CheckColorValue();

                                //Open the dialog, tab to hue, change hue, tab to accept button, activate it with space,
                                //send tab (since the dialog should be closed, this will transfer focus to the next
                                //input-color button)
                                driver.SendSpecialKeys(id, "EscapeEnterTabTabArrow_rightArrow_rightTabSpaceTab");
                                if (initial == CheckColorValue() || ActiveElement() == id)
                                {
                                    result += "\nUnable to accept with accept button via space";
                                }

                                initial = CheckColorValue();//the value hopefully changed above, but just to be safe

                                //Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                                //We don't have to worry about why the dialog closed here (button or global enter)
                                driver.SendSpecialKeys(id, "EscapeEnterTabTabArrow_rightArrow_rightTabEnterTab");
                                if (initial == CheckColorValue() || ActiveElement() == id)
                                {
                                    result += "\nUnable to accept with accept button via enter";
                                }
                            }
                        }

                        return result;
                    }),
                new TestData("input-date", "Edit",
                    keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement:
                        CheckCalendar(3)),
                new TestData("input-datetime-local", "Edit",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement:
                        CheckDatetimeLocal()),
                new TestData("input-email", "Edit", "email",
                    keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement: (elements, driver, ids) => {
                        return CheckValidation()(elements, driver, ids) + "\n" +
                            CheckClearButton()(elements, driver, ids);
                    }),
                new TestData("input-month", "Edit", keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement:
                        CheckCalendar(2)),
                new TestData("input-number", "Spinner", "number",
                    keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement:
                        CheckValidation()),
                new TestData("input-range", "Slider", keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7"},
                    additionalRequirement: (elements, driver, ids) => {
                        var result = "";

                        //keyboard interaction
                        foreach(var id in ids.Take(1)){
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
                        return result;
                    }),
                new TestData("input-search", "Edit", "search", keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7"},
                    additionalRequirement:
                        CheckClearButton()),
                new TestData("input-tel", "Edit", "telephone", keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7"},
                    additionalRequirement:
                        CheckClearButton()),
                new TestData("input-time", "Edit", keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement:
                        CheckCalendar(3, 2)),
                new TestData("input-url", "Edit", "url",
                        keyboardElements: new List<string> { "input1", "input2" },
                        additionalRequirement: (elements, driver, ids) =>
                            CheckValidation()(elements, driver, ids) +
                            CheckClearButton()(elements, driver, ids)),
                new TestData("input-week", "Edit", keyboardElements: new List<string> { "input1", "input2" },
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement:
                        CheckCalendar(2)),
                new TestData("main", "Group", "main", "Main", "main",
                    requiredNames:
                        new List<string>{
                            "title attribute 1",
                            "aria-label attribute 2",
                            "h1 referenced by aria-labelledby3",
                            "title attribute 4",
                            "aria-label attribute 6"
                        },
                    requiredDescriptions:
                        new List<string>{
                            "h1 referenced by aria-describedby5",
                            "title attribute 6"}),
                new TestData("mark", "Text", "mark",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute2",
                            "Element referenced by aria-labelledby attribute3",
                            "title attribute 4",
                            "aria-label attribute 6"
                        },
                    requiredDescriptions:
                        new List<string>{
                            "Element referenced by aria-describedby attribute5",
                            "title attribute 6"}),
                new TestData("meter", "Progressbar", "meter",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping meter 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"},
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement:
                        ((elements, driver, ids) => {
                            var result = "";
                            //readonly
                            if(!elements.All(element => element.GetProperties().Any(p => p.Contains("IsReadOnly")))){
                                result += "Not all elements were read only";
                            }

                            //rangevalue
                            foreach (var element in elements)
                            {
                                var patternName = "RangeValuePattern";

                                var patterned = GetPattern<IUIAutomationRangeValuePattern>(patternName, element);
                                if(patterned == null)
                                {
                                    result += "\nElement did not support " + patternName;
                                }
                                else
                                {
                                    if (patterned.CurrentMaximum - 100 > epsilon)
                                    {
                                        result += "\nElement did not have the correct max";
                                    }
                                    if (patterned.CurrentMinimum - 0 > epsilon)
                                    {
                                        result += "\nElement did not have the correct min";
                                    }
                                    var value = 83.5;//All the meters are set to this
                                    if (patterned.CurrentValue - value > epsilon)
                                    {
                                        result += "\nElement did not have the correct value";
                                    }
                                }
                            }

                            //value
                            foreach (var element in elements)
                            {
                                var patterned = GetPattern<IUIAutomationValuePattern>("ValuePattern", element);
                                if (patterned == null)
                                {
                                    result += "\nElement did not support ValuePattern";
                                }
                                else
                                {
                                    if (patterned.CurrentValue == null || patterned.CurrentValue != "Good")
                                    {
                                        result += "\nElement did not have value set";
                                    }
                                }
                            }

                            return result;
                        }),
                    searchStrategy: (element => element.GetPatterns().Contains("RangeValuePattern"))),//NB the ControlType is not used for searching this element
                new TestData("menuitem", null),
                new TestData("menupopup", null),
                new TestData("menutoolbar", null),
                new TestData("nav", "Group", "navigation", "Navigation", "navigation",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "h1 referenced by aria-labelledby3",
                            "title attribute 4",
                            "aria-label attribute 6"},
                    requiredDescriptions:
                        new List<string>{
                            "h1 referenced by aria-describedby5",
                            "title attribute 6"}),
                new TestData("output", "Group", "output",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping output 410",
                            "title attribute 5",
                            "label referenced by for/id attributes 7" },
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement: ((elements, driver, ids) => {
                        var result = string.Empty;

                        if (!elements.All(element => ((IUIAutomationElement5)element).CurrentLiveSetting == LiveSetting.Polite)){
                            result += "\nElement did not have LiveSetting = Polite";
                        }
                        var controllerForLengths = elements.ConvertAll(element => element.CurrentControllerFor != null ? element.CurrentControllerFor.Length : 0);
                        if (controllerForLengths.Count(cfl => cfl > 0) != 1)
                        {
                            result += "\nExpected 1 element with ControllerFor set. Found " + controllerForLengths.Count(cfl => cfl > 0);
                        }
                        return result;
                    })),
                new TestData("progress", "Progressbar",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping output 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7" },
                    requiredDescriptions:
                        new List<string>{
                            "p referenced by aria-describedby6",
                            "title attribute 7" },
                    additionalRequirement: (elements, driver, ids) => {
                    var result = string.Empty;

                    //rangevalue
                    foreach (var element in elements)
                    {
                        var patternName = "RangeValuePattern";

                        var patterned = GetPattern<IUIAutomationRangeValuePattern>(patternName, element);
                        if(patterned == null)
                        {
                            result += "\nElement did not support " + patternName;
                        }
                        else
                        {
                            if (patterned.CurrentMaximum - 100 > epsilon)
                            {
                                result += "\nElement did not have the correct max";
                            }
                            if (patterned.CurrentMinimum - 0 > epsilon)
                            {
                                result += "\nElement did not have the correct min";
                            }
                            var value = 22;//All the progress bars are set to this
                            if (patterned.CurrentValue - value > epsilon)
                            {
                                result += "\nElement did not have the correct value";
                            }
                        }
                    }

                    return result;
                    }),
                new TestData("section", "Group", "section", "Custom", "region",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"},
                    requiredDescriptions:
                        new List<string>{
                            "h1 referenced by aria-describedby6",
                            "title attribute 7" }),
                new TestData("summary", null),
                new TestData("time", "Text", "time",
                    requiredNames:
                        new List<string>{
                            "aria-label attribute2",
                            "Element referenced by aria-labelledby attribute 3",
                            "title attribute 4",
                            "aria-label attribute 6"
                        },
                    requiredDescriptions:
                        new List<string>{
                            "2015-10-01",
                            "2015-10-02",
                            "2015-10-03",
                            "2015-10-04",
                            "Element referenced by aria-describedby attribute",
                            "title attribute 6",
                        }),
                new TestData("track", "track",
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        driver.ExecuteScript(Javascript.Track, timeout);

                        return (bool)driver.ExecuteScript("return Modernizr.track && Modernizr.texttrackapi", timeout) ? "" :
                            "Element was not found to be supported by Modernizr";
                    }),
                    searchStrategy: (element => true)),
                new TestData("video", "Group", null, keyboardElements: new List<string> { "video1" },
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        return CheckChildNames(
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
                                "Full screen" })(elements, driver, ids) +
                       CheckVideoKeyboardInteractions(elements, driver, ids);
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

                        //make sure the button show up now that their parents are not hidden
                        HashSet<string> foundControlTypes;
                        elements = EdgeA11yTools.SearchChildren(browserElement, "Button", null, out foundControlTypes);
                        if (elements.Count(e => e.CurrentControlType != paneCode) != 1)
                        {
                            result += "\nFound " + elements.Count(e => e.CurrentControlType != paneCode) + " elements. Expected 1";
                        }

                        //remove aria-hidden attribute
                        driver.ExecuteScript(Javascript.RemoveAriaHidden, timeout);

                        //both buttons should now be visible, since both aria-hidden and hidden attribute are missing
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
                        var result = "";
                        driver.SendSpecialKeys("input1", "Enter");
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        foreach(var element in elements){//there can only be one
                            if(element.CurrentControllerFor == null || element.CurrentControllerFor.Length == 0){
                                result += "\nElement did not have controller for set";
                            }

                            if(element.CurrentIsRequiredForForm != 1){
                                result += "\nElement did not have IsRequiredForForm set to true";
                            }

                            if(element.CurrentHelpText == null || element.CurrentHelpText.Length == 0){
                                result += "\nElement did not have HelpText";
                            }
                        }

                        return result;
                    }),
                new TestData("placeholder-att", "Edit",
                    requiredNames:
                        new List<string> {
                            "placeholder text 1",
                            "Label text 2:",
                            "Label text 3:",
                            "placeholder text 4",
                            "placeholder text 5",
                            "aria-placeholder text 6" })
            };
        }

        /// <summary>
        /// Convert an element into a pattern interface
        /// </summary>
        /// <typeparam name="T">The pattern interface to implement</typeparam>
        /// <param name="patternName">The friendly name of the pattern</param>
        /// <param name="element">The element to convert</param>
        /// <returns>The element as an implementation of the pattern</returns>
        private static T GetPattern<T>(string patternName, IUIAutomationElement element)
        {
            List<string> patternNames;
            List<int> patternIds;

            patternNames = element.GetPatterns(out patternIds);
            if (!patternNames.Contains(patternName))
            {
                return default(T);
            }
            else
            {
                T pattern = (T)((IUIAutomationElement5)element).GetCurrentPattern(patternIds[patternNames.IndexOf(patternName)]);
                return pattern;
            }
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
            string result = "";

            Func<bool> VideoPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + videoId + "').paused", 0);
            Func<object> PauseVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').pause()", 0);
            Func<object> PlayVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').play()", 0);
            Func<double> GetVideoVolume = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').volume", 0).ParseMystery();
            Func<double, bool> VideoVolume = expected => Math.Abs(GetVideoVolume() - expected) < epsilon;
            Func<bool> VideoMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').muted", 0);
            Func<double> GetVideoElapsed = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').currentTime", 0).ParseMystery();
            Func<double, bool> VideoElapsed = expected => Math.Abs(GetVideoElapsed() - expected) < epsilon;
            Func<bool> IsVideoFullScreen = () => driver.ExecuteScript("return document.webkitFullscreenElement", 0) != null;

            var handler = new StructureChangedHandler();
            WaitForElement(handler, elements[0], "volume");

            //Case 1: tab to play button and play/pause
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
            //TODO test manually

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
                driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
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
        /// <param name="elements">The elements found on the page</param>
        /// <param name="driver">The driver</param>
        /// <param name="ids">The html ids of the elements on the page</param>
        /// <returns>An empty string if an element fails, otherwise an explanation</returns>
        private static string CheckAudioKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids)
        {
            string audioId = "audio1";
            string result = "";
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
        /// <param name="outputFields">A count of the number of output fields to test</param>
        /// <returns></returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, string> CheckCalendar(int fields, int outputFields = -1)
        {
            return new Func<List<IUIAutomationElement>, DriverManager, List<string>, string>((elements, driver, ids) =>
            {
                //set to the number of fields by default
                outputFields = outputFields == -1 ? fields : outputFields;

                var result = "";
                var previousControllerForElements = new HashSet<int>();
                foreach (var id in ids.Take(1))
                {
                    driver.SendSpecialKeys(id, "EnterEscapeEnterEnter");//Make sure that the element has focus (gets around weirdness in WebDriver)

                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                    Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                    var today = DateValue();

                    //Open the menu
                    driver.SendSpecialKeys(id, "EscapeEnterWait");

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
                    for (int i = 0; i < outputFields; i++)
                    {
                        if (newdatesplit[i] == todaysplit[i])
                        {
                            result += "\nNot all fields were changed by keyboard interaction.";
                        }
                    }

                    var fieldTabs = "";
                    for (var i = 0; i < fields; i++)
                    {
                        fieldTabs += "Tab";
                    }

                    driver.SendSpecialKeys(id, "EnterWait" + fieldTabs);

                    var initial = "";

                    //Check that the accept and cancel buttons are in the tab order
                    if (ActiveElement() != id)
                    {
                        result += "\nUnable to get to accept/dismiss buttons by tab";
                    }
                    else//only try to use the buttons if they're there
                    {
                        initial = DateValue();
                        //**Dismiss button**
                        //Open the dialog, change a field, tab to cancel button, activate it with space,
                        //check that tabbing moves to the previous button (on the page not the dialog)
                        driver.SendSpecialKeys(id, "EscapeEnterWaitArrow_down" + fieldTabs + "TabSpaceShiftTabShift");
                        if (initial != DateValue() || ActiveElement() == id)
                        {
                            result += "\nUnable to cancel with dismiss button via space";
                        }

                        //do the same as above, but activate the button with enter this time
                        driver.SendSpecialKeys(id, "EscapeEnterWaitArrow_down" + fieldTabs + "TabEnterShiftTabShift");
                        if (initial != DateValue() || ActiveElement() == id)
                        {
                            result += "\nUnable to cancel with dismiss button via enter";
                        }


                        //**Accept button**
                        initial = DateValue();

                        //Open the dialog, change a field, tab to accept button, activate it with space,
                        //send tab (since the dialog should be closed, this will transfer focus to the next
                        //input-color button)
                        driver.SendSpecialKeys(id, "EscapeEnterWaitArrow_down" + fieldTabs + "SpaceTab");
                        if (initial == DateValue() || ActiveElement() == id)
                        {
                            result += "\nUnable to accept with accept button via space. Found value: " + DateValue() + " with active element: " + ActiveElement();
                        }

                        initial = DateValue();//the value hopefully changed above, but just to be safe

                        //Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                        //We don't have to worry about why the dialog closed here (button or global enter)
                        driver.SendSpecialKeys(id, "EscapeEnterWaitArrow_down" + fieldTabs + "EnterTab");
                        if (initial == DateValue() || ActiveElement() == id)
                        {
                            result += "\nUnable to accept with accept button via enter. Found value: " + DateValue() + " with active element: " + ActiveElement();
                        }
                    }

                    //open with space, close with enter
                    initial = DateValue();
                    driver.SendSpecialKeys(id, "EscapeSpaceArrow_downEnter");
                    if (DateValue() == initial)
                    {
                        result += "\nUnable to open dialog with space";
                    }
                }

                foreach (var element in elements)
                {
                    var patternName = "ValuePattern";
                    var patterned = GetPattern<IUIAutomationValuePattern>(patternName, element);
                    if (patterned == null)
                    {
                        result += "\nElement did not support " + patternName;
                    }
                    else
                    {
                        if (patterned.CurrentValue == null || patterned.CurrentValue == "")
                        {
                            result += "\nElement did not have value.value set";
                        }
                    }
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
                var inputFields = new List<int> { 3, 3 };
                var outputFields = 5;
                var result = "";

                var previousControllerForElements = new HashSet<int>();
                foreach (var id in ids.Take(1))
                {
                    driver.SendSpecialKeys(id, "EnterEnterEscape");//Make sure that the element has focus (gets around weirdness in WebDriver)

                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                    Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                    driver.SendSpecialKeys(id, "EnterEnterTabEnterEnter");
                    var today = DateValue();

                    //Open the menu
                    driver.SendSpecialKeys(id, "EnterWait");

                    //Check ControllerFor
                    var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().ConvertAll(element => elements.IndexOf(element));
                    if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                    {
                        result += "\nElement controller for not set for id: " + id;
                    }

                    foreach (var count in inputFields)
                    {
                        //Change each field in the calendar
                        for (int i = 0; i < count; i++)
                        {
                            driver.SendSpecialKeys(id, "Arrow_downTab");
                        }

                        driver.SendSpecialKeys(id, "EnterWaitTab");
                    }

                    //Get the altered value, which should be one off the default
                    //for each field
                    var newdate = DateValue();
                    var newdatesplit = newdate.Split('-', ':', 'T');
                    var todaysplit = today.Split('-', ':', 'T');

                    //ensure that all fields have been changed
                    for (int i = 0; i < outputFields; i++)
                    {
                        if (newdatesplit[i] == todaysplit[i])
                        {
                            result += "\nNot all fields were changed by keyboard interaction.";
                        }
                    }

                    var initial = "";

                    for (var i = 0; i < inputFields.Count(); i++)
                    {
                        var fieldTabs = "";
                        for (var j = 0; j < inputFields[0]; j++)
                        {
                            fieldTabs += "Tab";
                        }

                        var secondPass = i == 1 ? "Tab" : "";

                        initial = DateValue();
                        //**Dismiss button**
                        //Open the dialog, change a field, tab to cancel button, activate it with space,
                        //check that tabbing moves to the previous button (on the page not the dialog)
                        driver.SendSpecialKeys(id, secondPass + "EnterWaitArrow_down" + fieldTabs + "TabSpaceShiftTabShift");
                        if (initial != DateValue())
                        {
                            result += "\nUnable to cancel with dismiss button via space";
                        }

                        //do the same as above, but activate the button with enter this time
                        driver.SendSpecialKeys(id, secondPass + "EnterWaitArrow_down" + fieldTabs + "TabEnterShiftTabShift");
                        if (initial != DateValue())
                        {
                            result += "\nUnable to cancel with dismiss button via enter";
                        }


                        //**Accept button**
                        initial = DateValue();

                        //Open the dialog, change a field, tab to accept button, activate it with space,
                        //send tab (since the dialog should be closed, this will transfer focus to the next
                        //button)
                        driver.SendSpecialKeys(id, "Escape" + secondPass + "EnterWaitArrow_down" + fieldTabs + "SpaceTab");
                        if (initial == DateValue())
                        {
                            result += "\nUnable to accept with accept button via space";
                        }

                        initial = DateValue();//the value hopefully changed above, but just to be safe

                        //Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                        //We don't have to worry about why the dialog closed here (button or global enter)
                        driver.SendSpecialKeys(id, "Escape" + secondPass + "EnterWaitArrow_down" + fieldTabs + "EnterTab");
                        if (initial == DateValue())
                        {
                            result += "\nUnable to accept with accept button via enter";
                        }
                    }

                    //open with space, close with enter
                    initial = DateValue();
                    driver.SendSpecialKeys(id, "EscapeSpaceWaitArrow_downEnter");
                    if (DateValue() == initial)
                    {
                        result += "\nUnable to open dialog with space";
                    }
                }

                foreach (var element in elements)
                {
                    var patternName = "ValuePattern";
                    var patterned = GetPattern<IUIAutomationValuePattern>(patternName, element);
                    if (patterned == null)
                    {
                        result += "\nElement did not support " + patternName;
                    }
                    else
                    {
                        if (patterned.CurrentValue == null || patterned.CurrentValue == "")
                        {
                            result += "\nElement did not have value.value set";
                        }
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
                    var result = "";
                    //The indices of the elements that have been found to be invalid before
                    var previouslyInvalid = new HashSet<int>();
                    foreach (var id in ids)
                    {
                        driver.SendKeys(id, "invalid");
                        driver.SendSpecialKeys(id, "EnterWait");

                        //Everything that is invalid on the page
                        //We search by both with an OR condition because it gives a better chance to
                        //find elements that are partially correct.
                        var invalid = elements.Where(e =>
                                        e.CurrentIsDataValidForForm == 0).Select(e => elements.IndexOf(e));

                        //Elements that are invalid for the first time
                        var newInvalid = invalid.DefaultIfEmpty(-1).FirstOrDefault(inv => !previouslyInvalid.Contains(inv));
                        if (newInvalid == -1)
                        {
                            return "\nElement failed to validate improper input";
                        }

                        if (elements[newInvalid].CurrentIsDataValidForForm != 0)
                        {
                            result += "\nElement did not have IsDataValidForForm set to false";
                        }

                        if (elements[newInvalid].CurrentHelpText == null || elements[newInvalid].CurrentHelpText.Length == 0)
                        {
                            result += "\nElement did not have HelpText";
                        }

                        if (elements[newInvalid].CurrentControllerFor.Length != 1)
                        {
                            return result + "\n" + id + " did not have 1 ControllerFor " + elements[newInvalid].CurrentControllerFor.Length;
                        }

                        var helpPane = elements[newInvalid].CurrentControllerFor.GetElement(0);
                        if (helpPane.CurrentControlType != new ElementConverter().GetElementCodeFromName("Pane"))
                        {
                            result += "Error message did not have correct ControlType";
                        }

                        previouslyInvalid.Add(newInvalid);
                    }

                    return result;
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
                var result = string.Empty;
                foreach (var element in elements)
                {
                    var names = element.GetChildNames(searchStrategy);

                    var expectedNotFound = requiredNames.Where(rn => !names.Contains(rn)).ToList();//get a list of all required names not found
                    var foundNotExpected = names.Where(n => !requiredNames.Contains(n)).ToList();//get a list of all found names that weren't required

                    if (strict && names.Count() != requiredNames.Count)
                    {
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
                    }
                }
                return result;
            };
        }

        /// <summary>
        /// Check that the clear button can be tabbed to and that it can be activated with space
        /// </summary>
        /// <returns></returns>
        public static Func<IEnumerable<IUIAutomationElement>, DriverManager, List<string>, string> CheckClearButton()
        {
            return (elements, driver, ids) =>
            {
                var result = "";
                Func<string, string> inputValue = (id) => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                Action<string> clearInput = (id) => driver.ExecuteScript("document.getElementById('" + id + "').value = ''", 0);

                foreach (var id in ids.Take(1))
                {
                    //Enter something, tab to the clear button, clear with space
                    driver.SendSpecialKeys(id, "xWait");
                    if (!elements.Any(e => e.GetAllDescendents().Any(d => d.CurrentName.ToLowerInvariant().Contains("clear value"))))
                    {
                        result += "\nUnable to find a clear button as a child of any element";
                    }
                    //Don't leave input which could cause problems with other tests
                    clearInput(id);
                }

                return result;
            };
        }

        /// <summary>
        /// Wait for a given element's structure to change before continuing
        /// </summary>
        /// <param name="element">The element to change</param>
        /// <param name="elementName">The child element to be added</param>
        /// <param name="timeout">How long to wait</param>
        /// <returns></returns>
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

        /// <summary>
        /// Wait for a given element's structure to change before continuing
        /// </summary>
        /// <param name="handler">If the caller already has a stucture changed handler, it can be passed in</param>
        /// <param name="element">The element to change</param>
        /// <param name="elementName">The child element to be added</param>
        /// <param name="timeout">How long to wait</param>
        /// <returns></returns>
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

        /// <summary>
        /// Wait for a condition that tests a double value
        /// </summary>
        /// <param name="conditionCheck">The condition checker</param>
        /// <param name="value">The double value to look for</param>
        /// <returns></returns>
        public static bool WaitForCondition(Func<double, bool> conditionCheck, double value)
        {
            for (var i = 0; i < 100; i++)
            {
                Thread.Sleep(50);
                if (conditionCheck(value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Wait for a condition that only returns a boolean value
        /// </summary>
        /// <param name="conditionCheck">The condition checker</param>
        /// <returns></returns>
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
