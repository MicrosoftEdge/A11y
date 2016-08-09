namespace Microsoft.Edge.A11y
{
    using Interop.UIAutomationCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using static ElementConverter;

    /// <summary>
    /// This is where the logic of the tests is stored
    /// </summary>
    class TestData
    {
        public const double epsilon = .001;

        /// <summary>
        /// The name of the test, which corresponds to the name of the html element
        /// </summary>
        private string _TestName;
        public string TestName
        {
            get
            {
                return _TestName;
            }
        }

        /// <summary>
        /// The name of the UIA control type we will use to search for the element
        /// </summary>
        private UIAControlType _ControlType;
        public UIAControlType ControlType
        {
            get
            {
                return _ControlType;
            }
        }

        /// <summary>
        /// The name of the UIA localized control type, which will be part of the test
        /// case if it is not null
        /// </summary>
        private string _LocalizedControlType;
        public string LocalizedControlType
        {
            get
            {
                return _LocalizedControlType;
            }
        }

        /// <summary>
        /// The name of the UIA landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        private UIALandmarkType _LandmarkType;
        public UIALandmarkType LandmarkType
        {
            get
            {
                return _LandmarkType;
            }
        }

        /// <summary>
        /// The name of the UIA localized landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        private string _LocalizedLandmarkType;
        public string LocalizedLandmarkType
        {
            get
            {
                return _LocalizedLandmarkType;
            }
        }

        /// <summary>
        /// A list of ids for all the elements that should be keyboard accessible (via tab)
        /// </summary>
        private List<string> _KeyboardElements;
        public List<string> KeyboardElements
        {
            get
            {
                return _KeyboardElements;
            }
        }

        /// <summary>
        /// If not null, this func will be used to test elements to see if they should be
        /// tested (instead of matching _ControlType).
        /// </summary>
        private Func<IUIAutomationElement, bool> _SearchStrategy;
        public Func<IUIAutomationElement, bool> SearchStrategy
        {
            get
            {
                return _SearchStrategy;
            }
        }

        /// <summary>
        /// A list of expected values which will be compared to the accessible names of
        /// the found elements.
        /// </summary>
        private List<string> _requiredNames;
        public List<string> RequiredNames
        {
            get
            {
                return _requiredNames;
            }
        }

        /// <summary>
        /// Same as above, but for accessible descriptions.
        /// </summary>
        private List<string> _RequiredDescriptions;
        public List<string> RequiredDescriptions
        {
            get
            {
                return _RequiredDescriptions;
            }
        }

        /// <summary>
        /// A func that allows extending the tests for specific elements. 
        /// </summary>
        private Func<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> _AdditionalRequirement;
        public Func<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> AdditionalRequirement
        {
            get
            {
                return _AdditionalRequirement;
            }
        }

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
            UIAControlType controlType,
            string localizedControlType = null,
            UIALandmarkType landmarkType = UIALandmarkType.Unknown,
            string localizedLandmarkType = null,
            List<string> keyboardElements = null,
            Func<IUIAutomationElement, bool> searchStrategy = null,
            List<string> requiredNames = null,
            List<string> requiredDescriptions = null,
            Func<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> additionalRequirement = null)
        {
            _TestName = testName;
            _ControlType = controlType;
            _LocalizedControlType = localizedControlType;
            _LandmarkType = landmarkType;
            _LocalizedLandmarkType = localizedLandmarkType;
            _KeyboardElements = keyboardElements;
            _SearchStrategy = searchStrategy;
            _requiredNames = requiredNames;
            _RequiredDescriptions = requiredDescriptions;
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
            const int timeout = 0;
            var uia = new CUIAutomation8();
            var walker = uia.RawViewWalker;

            return new List<TestData>{
                new TestData("article", UIAControlType.Group, "article",
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
                new TestData("aside", UIAControlType.Group, "aside", UIALandmarkType.Custom, "complementary",
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
                new TestData("audio", UIAControlType.Group, "audio",
                    additionalRequirement: ((elements, driver, ids) => {
                        var result = new TestCaseResultExt();

                        CheckChildNames(new List<string> {
                                "Play",
                                "Time elapsed",
                                "Seek",
                                "Time remaining",
                                "Mute",
                                "Volume"})(elements, driver, ids, result);

                        CheckAudioKeyboardInteractions(elements, driver, ids, result);
                        return result;
                        }
                    )),
                new TestData("canvas", UIAControlType.Image,
                    additionalRequirement: ((elements, driver, ids) => {
                        var result = new TestCaseResultExt();
                        var subdomElements = new List<IUIAutomationElement>();

                        foreach (var e in elements)
                        {
                            subdomElements.AddRange(e.GetAllDescendants((d) =>
                            {
                                var convertedRole =
                                    GetControlTypeFromCode(d.CurrentControlType);
                                return (convertedRole == UIAControlType.Button || convertedRole == UIAControlType.Text);
                            }));
                        }
                        if(subdomElements.Count() != 3)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("Unable to find subdom elements");
                        }

                        var featureDetectionScript = @"canvas = document.getElementById('myCanvas');
                                                        isSupported = !!(canvas.getContext && canvas.getContext('2d'));
                                                        isSupported = isSupported && !!(canvas.getContext('2d').drawFocusIfNeeded);
                                                        return isSupported;";

                        if(!(bool)driver.ExecuteScript(featureDetectionScript, timeout))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFailed feature detection");
                        }

                        return result;
                    })),
                new TestData("datalist", UIAControlType.Combobox, keyboardElements: new List<string> { "input1" },
                    additionalRequirement: ((elements, driver, ids) => {
                        Func<string, string> datalistValue = (id) => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                        var result = new TestCaseResultExt();

                        foreach(var element in elements)
                        {
                            var elementFive = (IUIAutomationElement5)element;
                            List<int> patternIds;
                            var names = elementFive.GetPatterns(out patternIds);

                            if (!names.Contains("SelectionPattern"))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not support SelectionPattern");
                            }
                            else {
                                var selectionPattern = (IUIAutomationSelectionPattern)elementFive.GetCurrentPattern(
                                    patternIds[names.IndexOf("SelectionPattern")]);

                                if(selectionPattern.CurrentCanSelectMultiple == 1)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nCanSelectMultiple set to true");
                                }
                            }
                        }
                        var previousControllerForElements = new HashSet<int>();

                        //keyboard a11y
                        foreach (var id in ids.Take(1))
                        {
                            var initial = datalistValue(id);
                            driver.SendSpecialKeys(id, WebDriverKey.Arrow_down);

                            var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().Select(element => elements.IndexOf(element));
                            if(controllerForElements.All(element => previousControllerForElements.Contains(element))){
                                result.Result = ResultType.Half;
                                result.AddInfo("Element controller for not set for id: " + id);
                            }

                            previousControllerForElements.Add(controllerForElements.First(element => !previousControllerForElements.Contains(element)));

                            driver.SendSpecialKeys(id, WebDriverKey.Enter);
                            if (datalistValue(id) != "Item value 1")
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("Unable to set the datalist with keyboard for element with id: " + id);
                                return result;
                            }
                        }

                        return result;
                    })),
                new TestData("figure", UIAControlType.Group, "figure",
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
                new TestData("figure-figcaption", UIAControlType.Text,//Control type is ignored
                    searchStrategy: element => true, //Verify this element via text range
                    additionalRequirement: ((elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            var logoText = "HTML5 logo 1";

                            //there will be only one, since element is the pane in this case
                            foreach(var element in elements) {
                                var five = (IUIAutomationElement5)
                                    walker.GetFirstChildElement(
                                        walker.GetFirstChildElement(elements[0]));//only have the pane element, take its grandchild
                                List<int> patternIds;
                                var names = five.GetPatterns(out patternIds);

                                if (!names.Contains("TextPattern"))
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nPane did not support TextPattern, unable to search");
                                    return result;
                                }

                                var textPattern = (IUIAutomationTextPattern)five.GetCurrentPattern(
                                    patternIds[names.IndexOf("TextPattern")]);

                                var documentRange = textPattern.DocumentRange;

                                var foundText = documentRange.GetText(1000);
                                if (!foundText.Contains(logoText))
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nText not found on page");
                                }

                                var foundControlTypes = new HashSet<UIAControlType>();
                                var figure = EdgeA11yTools.SearchChildren(element, UIAControlType.Group, null, out foundControlTypes);

                                var childRange = textPattern.RangeFromChild(figure[0]);

                                var childRangeText = childRange.GetText(1000).Trim();

                                if(childRangeText != logoText)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo(string.Format("\nUnable to find correct text range. Found '{0}' instead", childRangeText));
                                }
                            }

                            return result;
                        })),
                new TestData("footer", UIAControlType.Group,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Group
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
                        var result = new TestCaseResultExt();

                        if (elements.Count() != 7)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + elements.Count() + " elements, expected 7.");
                        }

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;
                        //same for landmark and localizedlandmark
                        foreach (var element in elements)
                        {
                            var five = element as IUIAutomationElement5;
                            var convertedLandmark = GetLandmarkTypeFromCode(five.CurrentLandmarkType);
                            var localizedLandmark = five.CurrentLocalizedLandmarkType;
                            if (convertedLandmark == UIALandmarkType.Custom)
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
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + convertedLandmarks + " elements with landmark type Custom, expected 1");
                        }

                        if (localizedLandmarks != 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + localizedLandmarks + " elements with localized landmark type content information, expected 1");
                        }

                        return result;
                    }),
                new TestData("header", UIAControlType.Group,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Group
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
                        var result = new TestCaseResultExt();

                        if (elements.Count() != 7)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + elements.Count() + " elements, expected 7.");
                        }

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;
                        var landmarkCode = 0;
                        //same for landmark and localizedlandmark
                        foreach (var element in elements)
                        {
                            var five = element as IUIAutomationElement5;
                            var convertedLandmark = GetLandmarkTypeFromCode(five.CurrentLandmarkType);
                            landmarkCode = five.CurrentLandmarkType;
                            var localizedLandmark = five.CurrentLocalizedLandmarkType;
                            if (convertedLandmark == UIALandmarkType.Custom)
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
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + convertedLandmarks + " elements with landmark type Custom, expected 1: " + landmarkCode);
                        }

                        if (localizedLandmarks != 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + localizedLandmarks + " elements with localized landmark type banner, expected 1");
                        }

                        return result;
                    }),
                new TestData("input-color", UIAControlType.Button, "color picker",
                    additionalRequirement: (elements, driver, ids) => {
                        var result = new TestCaseResultExt();

                        var previousControllerForElements = new HashSet<int>();
                        foreach(var id in ids.Take(1))
                        {
                            Func<string> CheckColorValue = () => (string) driver.ExecuteScript("return document.getElementById('"+ id + "').value", timeout);
                            Action<string> ChangeColorValue = (value) => driver.ExecuteScript("document.getElementById('"+ id + "').value = '" + value + "'", timeout);
                            Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                            var initial = CheckColorValue();
                            driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Enter, WebDriverKey.Escape, WebDriverKey.Enter, WebDriverKey.Enter });

                            //open dialog to check controllerfor
                            driver.SendSpecialKeys(id, WebDriverKey.Enter);
                            var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().Select(element => elements.IndexOf(element));
                            if(controllerForElements.All(element => previousControllerForElements.Contains(element))){
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement controller for not set for id: " + id);
                            }
                            else
                            {
                                //the element that corresponds to this id
                                var thisElement = elements[controllerForElements.First(element => !previousControllerForElements.Contains(element))];
                                if(thisElement.CurrentControllerFor.Length > 1){
                                    throw new Exception("\nMore than one ControllerFor present, test assumption failed");
                                }
                                var thisDialog = thisElement.CurrentControllerFor.GetElement(0);
                                var descendents = thisDialog.GetAllDescendants();

                                //sliders
                                var sliders = descendents.Where(d => GetControlTypeFromCode(d.CurrentControlType) == UIAControlType.Slider);
                                if(sliders.Count() != 3){
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nDialog did not have three slider elements");
                                }
                                else if (!sliders.All(s => s.GetPatterns().Contains("RangeValuePattern")))
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nDialog's sliders did not implement RangeValuePattern");
                                }

                                //buttons
                                if (descendents.Count(d => GetControlTypeFromCode(d.CurrentControlType) == UIAControlType.Button) != 2)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nDialog did not have two button elements");
                                }

                                //color well
                                var outputs = descendents.Where(d => GetControlTypeFromCode(d.CurrentControlType) == UIAControlType.Group && d.CurrentLocalizedControlType == "output");
                                if (outputs.Count() > 1)
                                {
                                    throw new Exception("Test assumption failed: expected color dialog to have at most one output");
                                }
                                else if (outputs.Count() == 0)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nCould not find output in color dialog");
                                }
                                else if (outputs.Count() == 1)
                                {
                                    var output = outputs.First();
                                    if (output.CurrentName == null || output.CurrentName == "")
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nColor dialog output did not have name set");
                                    }
                                    else
                                    {
                                        var initialName = output.CurrentName;

                                        driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Tab,
                                            WebDriverKey.Tab,
                                            WebDriverKey.Arrow_right,
                                            WebDriverKey.Arrow_right });

                                        if (output.CurrentName == initialName)
                                        {
                                            result.Result = ResultType.Half;
                                            result.AddInfo("\nColor dialog output did not change name when color changed: " + output.CurrentName);
                                        }

                                        ChangeColorValue("#000000");//Ensure that the color is clear before continuing
                                    }
                                }
                            }

                            //open with enter, close with escape
                            driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Escape,
                                WebDriverKey.Enter,
                                WebDriverKey.Tab,
                                WebDriverKey.Arrow_right,
                                WebDriverKey.Arrow_right,
                                WebDriverKey.Escape });
                            if (CheckColorValue() != initial)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to cancel with escape");
                            }

                            //open with enter, close with enter
                            driver.SendSpecialKeys(id, new List<WebDriverKey> {
                                WebDriverKey.Escape,
                                WebDriverKey.Enter,
                                WebDriverKey.Tab,
                                WebDriverKey.Tab,
                                WebDriverKey.Arrow_right,
                                WebDriverKey.Arrow_right,
                                WebDriverKey.Enter });
                            if (CheckColorValue() == initial)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to change value with arrow keys and submit with enter");
                            }

                            //open with space, close with enter
                            initial = CheckColorValue();
                            driver.SendSpecialKeys(id, new List<WebDriverKey> {
                                WebDriverKey.Escape,
                                WebDriverKey.Space,
                                WebDriverKey.Tab,
                                WebDriverKey.Tab,
                                WebDriverKey.Arrow_right,
                                WebDriverKey.Arrow_right,
                                WebDriverKey.Enter });
                            if (initial == CheckColorValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to open dialog with space");
                            }

                            initial = CheckColorValue();

                            driver.SendSpecialKeys(id, new List<WebDriverKey> {
                                WebDriverKey.Enter,
                                WebDriverKey.Tab,
                                WebDriverKey.Tab,
                                WebDriverKey.Tab });
                            if (ActiveElement() != id)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to reach accept/dismiss buttons via tab");
                            }
                            else//only try to use the buttons if they're there
                            {
                                //**Dismiss button**
                                //Open the dialog, change hue, tab to cancel button, activate it with space,
                                //check that tabbing moves to the previous button (on the page not the dialog)
                                driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Escape,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Space,
                                    WebDriverKey.Shift,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Shift });
                                if (initial != CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to cancel with dismiss button via space");
                                }

                                //do the same as above, but activate the button with enter this time
                                driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Escape,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Shift,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Shift });
                                if (initial != CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to cancel with dismiss button via enter");
                                }


                                //**Accept button**
                                initial = CheckColorValue();

                                //Open the dialog, tab to hue, change hue, tab to accept button, activate it with space,
                                //send tab (since the dialog should be closed, this will transfer focus to the next
                                //input-color button)
                                driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Escape,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Space,
                                    WebDriverKey.Tab });
                                if (initial == CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to accept with accept button via space");
                                }

                                initial = CheckColorValue();//the value hopefully changed above, but just to be safe

                                //Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                                //We don't have to worry about why the dialog closed here (button or global enter)
                                driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Escape,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Tab });
                                if (initial == CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to accept with accept button via enter");
                                }
                            }
                        }

                        return result;
                    }),
                new TestData("input-date", UIAControlType.Edit,
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
                        (element, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckCalendar(3)(element, driver, ids, result);
                            return result;
                        }),
                new TestData("input-datetime-local", UIAControlType.Edit,
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
                        (element, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckDatetimeLocal()(element, driver, ids, result);
                            return result;
                        }),
                new TestData("input-email", UIAControlType.Edit, "email",
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
                        var result = new TestCaseResultExt();
                        CheckValidation()(elements, driver, ids, result);
                        CheckClearButton()(elements, driver, ids, result);
                        return result;
                    }),
                new TestData("input-month", UIAControlType.Edit, keyboardElements: new List<string> { "input1", "input2" },
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
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckCalendar(2)(elements, driver, ids, result);
                            return result;
                        }),
                new TestData("input-number", UIAControlType.Spinner, "number",
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
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckValidation()(elements, driver, ids, result);
                            return result;
                        }),
                new TestData("input-range", UIAControlType.Slider, keyboardElements: new List<string> { "input1", "input2" },
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
                        var result = new TestCaseResultExt();

                        //keyboard interaction
                        foreach(var id in ids.Take(1)){
                            Func<int> RangeValue = () => (int) Int32.Parse((string) driver.ExecuteScript("return document.getElementById('" + id + "').value", 0));

                            var initial = RangeValue();
                            driver.SendSpecialKeys(id, WebDriverKey.Arrow_up);
                            if (initial == RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to increase range with arrow up");
                                continue;
                            }
                            driver.SendSpecialKeys(id, WebDriverKey.Arrow_down);
                            if (initial != RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to decrease range with arrow down");
                                continue;
                            }

                            driver.SendSpecialKeys(id, WebDriverKey.Arrow_right);
                            if (initial >= RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to increase range with arrow right");
                                continue;
                            }
                            driver.SendSpecialKeys(id, WebDriverKey.Arrow_left);
                            if (initial != RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to decrease range with arrow left");
                                continue;
                            }
                        }

                        //rangevalue pattern
                        foreach(var element in elements){
                            var rangeValuePattern = "RangeValuePattern";
                            if (!element.GetPatterns().Contains(rangeValuePattern)) {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not implement the RangeValuePattern");
                            }
                        }
                        return result;
                    }),
                new TestData("input-search", UIAControlType.Edit, "search", keyboardElements: new List<string> { "input1", "input2" },
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
                        (elements, driver, ids) => {
                            var result = new TestCaseResultExt();
                            CheckClearButton()(elements, driver, ids, result);
                            return result;
                        }),
                new TestData("input-tel", UIAControlType.Edit, "telephone", keyboardElements: new List<string> { "input1", "input2" },
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
                        (elements, driver, ids) => {
                            var result = new TestCaseResultExt();
                            CheckClearButton()(elements, driver, ids, result);
                            return result;
                        }),
                new TestData("input-time", UIAControlType.Edit, keyboardElements: new List<string> { "input1", "input2" },
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
                        (elements, driver, ids) => {
                            var result = new TestCaseResultExt();
                            CheckCalendar(3, 2)(elements, driver, ids, result);
                            return result;
                        }),
                new TestData("input-url", UIAControlType.Edit, "url",
                        keyboardElements: new List<string> { "input1", "input2" },
                        additionalRequirement:
                            (elements, driver, ids) => {
                                var result = new TestCaseResultExt();
                                CheckValidation()(elements, driver, ids, result);
                                CheckClearButton()(elements, driver, ids, result);
                                return result;
                            }),
                new TestData("input-week", UIAControlType.Edit, keyboardElements: new List<string> { "input1", "input2" },
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
                        (elements, driver, ids) => {
                            var result = new TestCaseResultExt();
                            CheckCalendar(2)(elements, driver, ids, result);
                            return result;
                        }),
                new TestData("main", UIAControlType.Group, "main", UIALandmarkType.Main, "main",
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
                new TestData("mark", UIAControlType.Text,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Text
                        && element.CurrentLocalizedControlType == "mark",
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
                new TestData("meter", UIAControlType.Progressbar, "meter",
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
                            var result = new TestCaseResultExt();
                            //readonly
                            if(!elements.All(element => element.GetProperties().Any(p => p.Contains("IsReadOnly")))){
                                result.Result = ResultType.Half;
                                result.AddInfo("Not all elements were read only");
                            }

                            //rangevalue
                            foreach (var element in elements)
                            {
                                var patternName = "RangeValuePattern";

                                var patterned = GetPattern<IUIAutomationRangeValuePattern>(patternName, element);
                                if(patterned == null)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not support " + patternName);
                                }
                                else
                                {
                                    if (patterned.CurrentMaximum - 100 > epsilon)
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nElement did not have the correct max");
                                    }
                                    if (patterned.CurrentMinimum - 0 > epsilon)
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nElement did not have the correct min");
                                    }
                                    var value = 83.5;//All the meters are set to this
                                    if (patterned.CurrentValue - value > epsilon)
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nElement did not have the correct value");
                                    }
                                }
                            }

                            //value
                            foreach (var element in elements)
                            {
                                var patterned = GetPattern<IUIAutomationValuePattern>("ValuePattern", element);
                                if (patterned == null)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not support ValuePattern");
                                }
                                else
                                {
                                    if (patterned.CurrentValue == null || patterned.CurrentValue != "Good")
                                    {
                                         result.Result = ResultType.Half;
                                        result.AddInfo("\nElement did not have value set");
                                    }
                                }
                            }

                            return result;
                        }),
                    searchStrategy: (element => element.GetPatterns().Contains("RangeValuePattern"))),//NB the ControlType is not used for searching this element
                new TestData("nav", UIAControlType.Group, "navigation", UIALandmarkType.Navigation, "navigation",
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
                new TestData("output", UIAControlType.Group, "output",
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
                        var result = new TestCaseResultExt();

                        if (!elements.All(element => ((IUIAutomationElement5)element).CurrentLiveSetting == LiveSetting.Polite)){
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have LiveSetting = Polite");
                        }
                        var controllerForLengths = elements.Select(element => element.CurrentControllerFor != null ? element.CurrentControllerFor.Length : 0);
                        if (controllerForLengths.Count(cfl => cfl > 0) != 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nExpected 1 element with ControllerFor set. Found " + controllerForLengths.Count(cfl => cfl > 0));
                        }
                        return result;
                    })),
                new TestData("progress", UIAControlType.Progressbar,
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
                        var result = new TestCaseResultExt();

                        //rangevalue
                        foreach (var element in elements)
                        {
                            var patternName = "RangeValuePattern";

                            var patterned = GetPattern<IUIAutomationRangeValuePattern>(patternName, element);
                            if(patterned == null)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not support " + patternName);
                            }
                            else
                            {
                                if (patterned.CurrentMaximum - 100 > epsilon)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not have the correct max");
                                }
                                if (patterned.CurrentMinimum - 0 > epsilon)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not have the correct min");
                                }
                                var value = 22;//All the progress bars are set to this
                                if (patterned.CurrentValue - value > epsilon)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not have the correct value");
                                }
                            }
                        }

                        return result;
                    }),
                new TestData("section", UIAControlType.Group, "section", UIALandmarkType.Custom, "region",
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
                new TestData("time", UIAControlType.Text,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Text
                        && element.CurrentLocalizedControlType == "time",
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
                new TestData("track", UIAControlType.Unknown,
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();
                        driver.ExecuteScript(Javascript.Track, timeout);

                        if(!(bool)driver.ExecuteScript("return Modernizr.track && Modernizr.texttrackapi", timeout)) {
                            result.Result = ResultType.Half;
                            result.AddInfo("Element was not found to be supported by Modernizr");
                        }
                        return result;
                    }),
                    searchStrategy: (element => true)),
                new TestData("video", UIAControlType.Group, null, keyboardElements: new List<string> { "video1" },
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();
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
                                "Full screen" })(elements, driver, ids, result);
                       CheckVideoKeyboardInteractions(elements, driver, ids, result);
                        return result;
                    })),
                    new TestData("hidden-att", UIAControlType.Button, null,
                    additionalRequirement: ((elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        var browserElement = EdgeA11yTools.FindBrowserDocument(0);

                        if (elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane) != 0)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("Found " + elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane) + " elements. Expected 0");
                            return result;
                        }

                        //Make sure the text isn't showing up on the page
                        var five = (IUIAutomationElement5)
                            walker.GetFirstChildElement(
                                walker.GetFirstChildElement(elements[0]));//only have the pane element, take its grandchild

                        List<int> patternIds;
                        var names = five.GetPatterns(out patternIds);
                        var textPatternString = "TextPattern";
                        if (!names.Contains(textPatternString))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("Pane did not support TextPattern, unable to search");
                            return result;
                        }

                        var textPattern = (IUIAutomationTextPattern)five.GetCurrentPattern(
                            patternIds[names.IndexOf(textPatternString)]);

                        var documentRange = textPattern.DocumentRange;

                        var foundText = documentRange.GetText(1000);
                        if(foundText.Contains("HiDdEn TeXt"))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound text that should have been hidden");
                        }

                        //remove hidden attribute
                        driver.ExecuteScript(Javascript.RemoveHidden, timeout);

                        //make sure the button show up now that their parents are not hidden
                        HashSet<UIAControlType> foundControlTypes;
                        elements = EdgeA11yTools.SearchChildren(browserElement, UIAControlType.Button, null, out foundControlTypes);
                        if (elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane) != 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane) + " elements. Expected 1");
                        }

                        //remove aria-hidden attribute
                        driver.ExecuteScript(Javascript.RemoveAriaHidden, timeout);

                        //both buttons should now be visible, since both aria-hidden and hidden attribute are missing
                        elements = EdgeA11yTools.SearchChildren(browserElement, UIAControlType.Button, null, out foundControlTypes);
                        if (elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane) != 2)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane) + " elements. Expected 2");
                        }

                        return result;
                    }),
                    searchStrategy: (element => true)),//take the pane
                new TestData("required-att", UIAControlType.Edit,
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();
                        driver.SendSpecialKeys("input1", WebDriverKey.Enter);
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        foreach(var element in elements){//there can only be one
                            if(element.CurrentControllerFor == null || element.CurrentControllerFor.Length == 0){
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not have controller for set");
                            }

                            if(element.CurrentIsRequiredForForm != 1){
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not have IsRequiredForForm set to true");
                            }

                            if(element.CurrentHelpText == null || element.CurrentHelpText.Length == 0){
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not have HelpText");
                            }
                        }

                        return result;
                    }),
                new TestData("placeholder-att", UIAControlType.Edit,
                    requiredNames:
                        new List<string> {
                            "placeholder text 1",
                            "Label text 2:",
                            "Label text 3:",
                            "placeholder text 4",
                            "placeholder text 5",
                            "aria-placeholder text 6" },
                    requiredDescriptions:
                        new List<string> {
                            "placeholder text 2",
                            "placeholder text 3",
                            "title text 4",
                            })
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
        /// Helper method to tab until an element whose name contains the given string
        /// reports that it has focus
        /// </summary>
        /// <param name="parent">The parent of the target element</param>
        /// <param name="name">A string which the target element's name will contain</param>
        /// <param name="tabId">The id to send tabs to</param>
        /// <param name="driver">The WebDriver</param>
        /// <returns>true if the element was found, false otherwise</returns>
        private static bool TabToElementByName(IUIAutomationElement parent, string name, string tabId, DriverManager driver)
        {
            var tabs = 0;
            var resets = 0;
            var element = parent.GetAllDescendants(e => e.CurrentName.Contains(name)).First();
            while (!(bool)element.GetCurrentPropertyValue(GetPropertyCode(UIAProperty.HasKeyboardFocus)))
            {
                driver.SendSpecialKeys(tabId, WebDriverKey.Tab);
                if (++tabs > 20)
                {
                    Javascript.ClearFocus(driver, 0);
                    tabs = 0;
                    resets++;
                    if (resets > 5)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Check basic keyboard interactions for the video control
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="driver"></param>
        /// <param name="ids"></param>
        /// <returns>An empty string if an element fails, otherwise an explanation</returns>
        private static void CheckVideoKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids, TestCaseResultExt result)
        {
            string videoId = "video1";

            Func<bool> VideoPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + videoId + "').paused", 0);
            Func<object> PauseVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').pause()", 0);
            Func<object> PlayVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').play()", 0);
            Func<double> GetVideoVolume = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').volume", 0).ParseMystery();
            Func<double, bool> VideoVolume = expected => Math.Abs(GetVideoVolume() - expected) < epsilon;
            Func<bool> VideoMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').muted", 0);
            Func<double> GetVideoElapsed = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').currentTime", 0).ParseMystery();
            Func<double, bool> VideoElapsed = expected => Math.Abs(GetVideoElapsed() - expected) < epsilon;
            Func<bool> IsVideoFullScreen = () => driver.ExecuteScript("return document.webkitFullscreenElement", 0) != null;
            Func<bool> IsVideoLoaded = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').readyState == 4", 0);

            if (!WaitForCondition(IsVideoLoaded, attempts: 40))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\nVideo did not load after 20 seconds");
                return;
            }

            //Case 1: tab to play button and play/pause
            TabToElementByName(elements[0], "Play", videoId, driver);
            driver.SendSpecialKeys(videoId, WebDriverKey.Space);
            if (!WaitForCondition(VideoPlaying))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was not playing after spacebar on play button\n");
                PlayVideo();
            }
            driver.SendSpecialKeys(videoId, WebDriverKey.Enter);
            if (!WaitForCondition(VideoPlaying, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was not paused after enter on play button\n");
                PauseVideo();
            }

            //Case 2: Volume and mute
            Javascript.ScrollIntoView(driver, 0);
            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Mute", videoId, driver);
            driver.Screenshot("before_enter_to_mute");
            driver.SendSpecialKeys(videoId, WebDriverKey.Enter);//mute
            driver.Screenshot("after_enter_to_mute");
            if (!WaitForCondition(VideoMuted))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tEnter did not mute the video\n");
            }

            WaitForCondition(() =>
                elements[0].GetAllDescendants().Any(e => e.CurrentName == "Unmute")
            );

            driver.Screenshot("before_enter_to_unmute");
            driver.SendSpecialKeys(videoId, WebDriverKey.Enter);//unmute
            driver.Screenshot("after_enter_to_unmute");
            if (!WaitForCondition(VideoMuted, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tEnter did not unmute the video\n");
            }
            var initial = GetVideoVolume();
            driver.SendSpecialKeys(videoId, new List<WebDriverKey> { WebDriverKey.Arrow_down, WebDriverKey.Arrow_down });//volume down
            if (!WaitForCondition(VideoVolume, initial - 0.1))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not decrease with arrow keys\n");
            }
            driver.SendSpecialKeys(videoId, new List<WebDriverKey> { WebDriverKey.Arrow_up, WebDriverKey.Arrow_up });//volume up
            if (!WaitForCondition(VideoVolume, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not increase with arrow keys\n");
            }

            //Case 3: Audio selection
            //TODO test manually

            //Case 4: Progress and seek
            if (VideoPlaying())
            { //this should not be playing
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was playing when it shouldn't have been\n");
            }
            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Seek", videoId, driver);
            initial = GetVideoElapsed();
            driver.SendSpecialKeys(videoId, WebDriverKey.Arrow_right); //skip ahead
            if (!WaitForCondition(VideoElapsed, initial + 10))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip forward with arrow right\n");
            }

            driver.SendSpecialKeys(videoId, WebDriverKey.Arrow_left); //skip back
            if (!WaitForCondition(VideoElapsed, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip back with arrow left\n");
            }

            //Case 5: Progress and seek on remaining time
            if (VideoPlaying())
            { //this should not be playing
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was playing when it shouldn't have been\n");
            }
            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Seek", videoId, driver);
            initial = GetVideoElapsed();
            driver.SendSpecialKeys(videoId, WebDriverKey.Arrow_right); //skip ahead
            if (!WaitForCondition(VideoElapsed, initial + 10))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip forward with arrow right\n");
            }

            driver.SendSpecialKeys(videoId, WebDriverKey.Arrow_left); //skip back
            if (!WaitForCondition(VideoElapsed, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip back with arrow left\n");
                driver.SendSpecialKeys(videoId, WebDriverKey.Arrow_left); //skip back
            }

            //Case 6: Full screen
            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Full screen", videoId, driver);
            driver.Screenshot("before_enter_to_fullscreen");
            driver.SendSpecialKeys(videoId, WebDriverKey.Enter); //enter fullscreen mode
            driver.Screenshot("after_enter_to_fullscreen");
            if (!WaitForCondition(IsVideoFullScreen))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not enter FullScreen mode\n");
            }
            driver.Screenshot("before_escape_from_fullscreen");
            driver.SendSpecialKeys(videoId, WebDriverKey.Escape);
            driver.Screenshot("after_escape_from_fullscreen");
            if (!WaitForCondition(IsVideoFullScreen, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not exit FullScreen mode\n");
            }
        }

        /// <summary>
        /// Check basic keyboard interactions for the audio control
        /// </summary>
        /// <param name="elements">The elements found on the page</param>
        /// <param name="driver">The driver</param>
        /// <param name="ids">The html ids of the elements on the page</param>
        /// <returns>An empty string if an element fails, otherwise an explanation</returns>
        private static void CheckAudioKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids, TestCaseResultExt result)
        {
            string audioId = "audio1";
            Func<bool> AudioPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + audioId + "').paused", 0);
            Func<object> PauseAudio = () => driver.ExecuteScript("!document.getElementById('" + audioId + "').pause()", 0);
            Func<object> PlayAudio = () => driver.ExecuteScript("!document.getElementById('" + audioId + "').play()", 0);
            Func<double> GetAudioVolume = () => driver.ExecuteScript("return document.getElementById('" + audioId + "').volume", 0).ParseMystery();
            Func<double, bool> AudioVolume = expected => Math.Abs(GetAudioVolume() - expected) < epsilon;
            Func<bool> AudioMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + audioId + "').muted", 0);
            Func<double> GetAudioElapsed = () => driver.ExecuteScript("return document.getElementById('" + audioId + "').currentTime", 0).ParseMystery();
            Func<double, bool> AudioElapsed = expected => Math.Abs(GetAudioElapsed() - expected) < epsilon;
            Func<bool> IsAudioLoaded = () => (bool)driver.ExecuteScript("return document.getElementById('" + audioId + "').readyState == 4", 0);

            if (!WaitForCondition(IsAudioLoaded, attempts: 40))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\nAudio did not load after 20 seconds");
                return;
            }

            //Case 1: Play/Pause
            driver.SendTabs(audioId, 1); //Tab to play button
            driver.SendSpecialKeys(audioId, WebDriverKey.Enter);
            if (!WaitForCondition(AudioPlaying))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not play with enter\n");
                PlayAudio();
            }

            driver.SendSpecialKeys(audioId, WebDriverKey.Space);
            if (!WaitForCondition(AudioPlaying, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not pause with space\n");
                PauseAudio();
            }

            //Case 2: Seek
            if (AudioPlaying())
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio was playing when it shouldn't have been\n");
            }
            driver.SendTabs(audioId, 3);
            var initial = GetAudioElapsed();
            driver.SendSpecialKeys(audioId, WebDriverKey.Arrow_right);
            if (!WaitForCondition(AudioElapsed, initial + 10))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not skip forward with arrow right\n");
            }
            driver.SendSpecialKeys(audioId, WebDriverKey.Arrow_left);
            if (!WaitForCondition(AudioElapsed, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not skip back with arrow left\n");
            }

            //Case 3: Volume and mute
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(audioId, 5);
            initial = GetAudioVolume();
            driver.SendSpecialKeys(audioId, WebDriverKey.Arrow_down);
            if (!WaitForCondition(AudioVolume, initial - .05))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not decrease with arrow down\n");
            }

            driver.SendSpecialKeys(audioId, WebDriverKey.Arrow_up);
            if (!WaitForCondition(AudioVolume, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not increase with arrow up\n");
            }

            driver.SendSpecialKeys(audioId, WebDriverKey.Enter);
            if (!WaitForCondition(AudioMuted))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio was not muted by enter on the volume control\n");
            }
            driver.SendSpecialKeys(audioId, WebDriverKey.Enter);
            if (!WaitForCondition(AudioMuted, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio was not unmuted by enter on the volume control\n");
            }
        }

        /// <summary>
        /// Test all date/time elements except for datetime-local, which is tested by the
        /// amended method below.
        /// </summary>
        /// <param name="fields">A count of the number of fields to test</param>
        /// <param name="outputFields">A count of the number of output fields to test</param>
        /// <returns></returns>
        public static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckCalendar(int fields, int outputFields = -1)
        {
            return ((elements, driver, ids, result) =>
            {
                //set to the number of fields by default
                outputFields = outputFields == -1 ? fields : outputFields;

                var previousControllerForElements = new HashSet<int>();
                foreach (var id in ids.Take(1))
                {
                    driver.SendSpecialKeys(id, new List<WebDriverKey> {
                        WebDriverKey.Enter,
                        WebDriverKey.Escape,
                        WebDriverKey.Enter,
                        WebDriverKey.Enter });//Make sure that the element has focus (gets around weirdness in WebDriver)

                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                    Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                    var today = DateValue();

                    //Open the menu
                    driver.SendSpecialKeys(id, new List<WebDriverKey> {
                        WebDriverKey.Escape,
                        WebDriverKey.Enter,
                        WebDriverKey.Wait });

                    //Check ControllerFor
                    var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().Select(element => elements.IndexOf(element));
                    if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement controller for not set for id: " + id);
                    }

                    //Change each field in the calendar
                    for (int i = 0; i < fields; i++)
                    {
                        driver.SendSpecialKeys(id, new List<WebDriverKey> {
                            WebDriverKey.Arrow_down,
                            WebDriverKey.Tab });
                    }


                    //Close the menu (only necessary for time)
                    driver.SendSpecialKeys(id, WebDriverKey.Enter);

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
                            result.Result = ResultType.Half;
                            result.AddInfo("\nNot all fields were changed by keyboard interaction.");
                        }
                    }

                    var fieldTabs = new List<WebDriverKey>();
                    for (var i = 0; i < fields; i++)
                    {
                        fieldTabs.Add(WebDriverKey.Tab);
                    }

                    var keys = new List<WebDriverKey> { WebDriverKey.Enter, WebDriverKey.Wait };
                    keys.AddRange(fieldTabs);
                    driver.SendSpecialKeys(id, keys);

                    var initial = "";

                    //Check that the accept and cancel buttons are in the tab order
                    if (ActiveElement() != id)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nUnable to get to accept/dismiss buttons by tab");
                    }
                    else//only try to use the buttons if they're there
                    {
                        initial = DateValue();
                        //**Dismiss button**
                        //Open the dialog, change a field, tab to cancel button, activate it with space,
                        //check that tabbing moves to the previous button (on the page not the dialog)
                        keys = new List<WebDriverKey> {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Tab,
                                WebDriverKey.Space,
                                WebDriverKey.Shift,
                                WebDriverKey.Tab,
                                WebDriverKey.Shift});
                        driver.SendSpecialKeys(id, keys);
                        if (initial != DateValue() || ActiveElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via space");
                        }

                        //do the same as above, but activate the button with enter this time
                        keys = new List<WebDriverKey> {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(new List<WebDriverKey> {
                            WebDriverKey.Tab,
                            WebDriverKey.Enter,
                            WebDriverKey.Shift,
                            WebDriverKey.Tab,
                            WebDriverKey.Shift });
                        driver.SendSpecialKeys(id, keys);
                        if (initial != DateValue() || ActiveElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via enter");
                        }

                        //**Accept button**
                        initial = DateValue();

                        //Open the dialog, change a field, tab to accept button, activate it with space,
                        //send tab (since the dialog should be closed, this will transfer focus to the next
                        //input-color button)
                        keys = new List<WebDriverKey> {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(new List<WebDriverKey> {
                            WebDriverKey.Space,
                            WebDriverKey.Tab });
                        driver.SendSpecialKeys(id, keys);
                        if (initial == DateValue() || ActiveElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via space. Found value: " + DateValue() + " with active element: " + ActiveElement());
                        }

                        initial = DateValue();//the value hopefully changed above, but just to be safe

                        //Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                        //We don't have to worry about why the dialog closed here (button or global enter)
                        keys = new List<WebDriverKey> {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(new List<WebDriverKey> {
                            WebDriverKey.Enter,
                            WebDriverKey.Tab });
                        driver.SendSpecialKeys(id, keys);
                        if (initial == DateValue() || ActiveElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via enter. Found value: " + DateValue() + " with active element: " + ActiveElement());
                        }
                    }

                    //open with space, close with enter
                    initial = DateValue();
                    driver.SendSpecialKeys(id, new List<WebDriverKey> {
                        WebDriverKey.Escape,
                        WebDriverKey.Space,
                        WebDriverKey.Arrow_down,
                        WebDriverKey.Enter });
                    if (DateValue() == initial)
                    {
                        result.AddInfo("\nUnable to open dialog with space");
                    }
                }

                foreach (var element in elements)
                {
                    var patternName = "ValuePattern";
                    var patterned = GetPattern<IUIAutomationValuePattern>(patternName, element);
                    if (patterned == null)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement did not support " + patternName);
                    }
                    else
                    {
                        if (patterned.CurrentValue == null || patterned.CurrentValue == "")
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have value.value set");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Test the datetime-local input element with an amended version of the method
        /// above.
        /// </summary>
        /// <returns></returns>
        public static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckDatetimeLocal()
        {
            return ((elements, driver, ids, result) =>
            {
                var inputFields = new List<int> { 3, 3 };
                var outputFields = 5;

                var previousControllerForElements = new HashSet<int>();
                foreach (var id in ids.Take(1))
                {
                    driver.SendSpecialKeys(id, new List<WebDriverKey> {
                        WebDriverKey.Enter,
                        WebDriverKey.Enter,
                        WebDriverKey.Escape });//Make sure that the element has focus (gets around weirdness in WebDriver)

                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                    Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                    driver.SendSpecialKeys(id, new List<WebDriverKey> {
                        WebDriverKey.Tab,
                        WebDriverKey.Enter,
                        WebDriverKey.Enter,
                        WebDriverKey.Tab,
                        WebDriverKey.Enter,
                        WebDriverKey.Enter });
                    var today = DateValue();

                    //Open the date menu
                    driver.SendSpecialKeys(id, new List<WebDriverKey> {
                        WebDriverKey.Shift,
                        WebDriverKey.Tab,
                        WebDriverKey.Enter,
                        WebDriverKey.Wait });

                    //Check ControllerFor
                    var controllerForElements = elements.Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0).ToList().Select(element => elements.IndexOf(element));
                    if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement controller for not set for id: " + id);
                    }

                    for (var i = 0; i < inputFields.Count;)//NB ++i later
                    {
                        //Change each field in the calendar
                        for (var j = 0; j < inputFields[i]; j++)
                        {
                            driver.SendSpecialKeys(id, new List<WebDriverKey> { WebDriverKey.Arrow_down,
                                WebDriverKey.Tab });
                        }
                        driver.SendSpecialKeys(id, new List<WebDriverKey> {
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Tab });
                        // send Enter to open menu
                        if (++i != inputFields.Count)
                        {
                            // expand menu
                            driver.SendSpecialKeys(id, WebDriverKey.Enter);
                        }
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
                            result.Result = ResultType.Half;
                            result.AddInfo("\nNot all fields were changed by keyboard interaction.");
                        }
                    }

                    var initial = "";

                    for (var i = 0; i < inputFields.Count(); i++)
                    {
                        var fieldTabs = new List<WebDriverKey>();
                        for (var j = 0; j < inputFields[0]; j++)
                        {
                            fieldTabs.Add(WebDriverKey.Tab);
                        }

                        var secondPass = i == 1;

                        initial = DateValue();
                        //**Dismiss button**
                        //Open the dialog, change a field, tab to cancel button, activate it with space,
                        //check that tabbing moves to the previous button (on the page not the dialog)
                        var keys = new List<WebDriverKey>();
                        if (secondPass)
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Tab,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Space,
                                WebDriverKey.Wait,
                                WebDriverKey.Shift,
                                WebDriverKey.Tab,
                                WebDriverKey.Shift });
                        }
                        else//same as above except without the tab to start
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Space,
                                WebDriverKey.Wait,
                                WebDriverKey.Shift,
                                WebDriverKey.Tab,
                                WebDriverKey.Shift });
                        }
                        driver.SendSpecialKeys(id, keys);

                        if (initial != DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via space");
                        }

                        //do the same as above, but activate the button with enter this time
                        if (secondPass)
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Tab,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Shift,
                                WebDriverKey.Tab,
                                WebDriverKey.Shift });
                        }
                        else//same as above except without the tab to start
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Shift,
                                WebDriverKey.Tab,
                                WebDriverKey.Shift });
                        }
                        driver.SendSpecialKeys(id, keys);

                        if (initial != DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via enter");
                        }


                        //**Accept button**
                        initial = DateValue();

                        //Open the dialog, change a field, tab to accept button, activate it with space,
                        //send tab (since the dialog should be closed, this will transfer focus to the next
                        //button)
                        if (secondPass)
                        {
                            keys = new List<WebDriverKey> {
                                WebDriverKey.Escape,
                                WebDriverKey.Tab,
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down,
                                WebDriverKey.Wait };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Wait,
                                WebDriverKey.Space,
                                WebDriverKey.Wait,
                                WebDriverKey.Tab});
                        }
                        else
                        {
                            keys = new List<WebDriverKey> {
                                WebDriverKey.Escape,
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down,
                                WebDriverKey.Wait };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Wait,
                                WebDriverKey.Space,
                                WebDriverKey.Wait,
                                WebDriverKey.Tab});
                        }
                        driver.SendSpecialKeys(id, keys);
                        if (initial == DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via space");
                        }

                        initial = DateValue();//the value hopefully changed above, but just to be safe

                        //Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                        //We don't have to worry about why the dialog closed here (button or global enter)
                        if (secondPass)
                        {
                            keys = new List<WebDriverKey> {
                                WebDriverKey.Escape,
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Enter,
                                WebDriverKey.Tab});
                        }
                        else
                        {
                            keys = new List<WebDriverKey> {
                                WebDriverKey.Escape,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey> {
                                WebDriverKey.Enter,
                                WebDriverKey.Tab});
                        }
                        driver.SendSpecialKeys(id, keys);
                        if (initial == DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via enter");
                        }
                    }

                    //open with space, close with enter
                    initial = DateValue();
                    driver.SendSpecialKeys(id, new List<WebDriverKey> {
                        WebDriverKey.Escape,
                        WebDriverKey.Space,
                        WebDriverKey.Wait,
                        WebDriverKey.Wait,
                        WebDriverKey.Arrow_down,
                        WebDriverKey.Enter });
                    if (DateValue() == initial)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nUnable to open dialog with space");
                    }
                }

                foreach (var element in elements)
                {
                    var patternName = "ValuePattern";
                    var patterned = GetPattern<IUIAutomationValuePattern>(patternName, element);
                    if (patterned == null)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement did not support " + patternName);
                    }
                    else
                    {
                        if (patterned.CurrentValue == null || patterned.CurrentValue == "")
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have value.value set");
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Func factory for checking that when invalid input is entered into a form,
        /// an error message appears.
        /// </summary>
        /// <returns></returns>
        public static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckValidation()
        {
            return (elements, driver, ids, result) =>
                {
                    //The indices of the elements that have been found to be invalid before
                    foreach (var id in ids.Take(1))
                    {
                        Javascript.ScrollIntoView(driver, 0);

                        driver.SendKeys(id, "invalid");
                        driver.SendSpecialKeys(id, WebDriverKey.Enter);

                        //Everything that is invalid on the page
                        //We search by both with an OR condition because it gives a better chance to
                        //find elements that are partially correct.
                        var invalid = elements.Where(e =>
                                        e.CurrentIsDataValidForForm == 0);

                        if (invalid.Count() > 1)
                        {
                            throw new Exception("Test assumption failed, multiple elements were invalid");
                        }

                        if (invalid.Count() < 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement failed to validate improper input");
                        }

                        var invalidElement = invalid.First();

                        if (!WaitForCondition(() => invalidElement.CurrentControllerFor.Length == 1, () => driver.SendSpecialKeys(id, WebDriverKey.Enter)))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\n" + id + " did not have 1 ControllerFor " + invalidElement.CurrentControllerFor.Length);
                            return;
                        }

                        if (invalidElement.CurrentIsDataValidForForm != 0)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have IsDataValidForForm set to false");
                        }

                        if (invalidElement.CurrentHelpText == null || invalidElement.CurrentHelpText.Length == 0)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have HelpText");
                        }

                        try
                        {
                            var helpPane = invalidElement.CurrentControllerFor.GetElement(0);
                            if (GetControlTypeFromCode(helpPane.CurrentControlType) != UIAControlType.Pane)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nError message did not have correct ControlType");
                            }
                        }
                        catch
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to get controller for");
                        }
                    }
                };
        }

        /// <summary>
        /// Func factory for checking that the required child elements are in the accessibility tree.
        /// </summary>
        /// <param name="requiredNames">The names of the elements to search for</param>
        /// <returns>A Func that can be used to verify whether the elements in the list are child elements</returns>
        public static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckChildNames(List<string> requiredNames,
            bool strict = false,
            Func<IUIAutomationElement, bool> searchStrategy = null)
        {
            return (elements, driver, ids, result) =>
            {
                foreach (var element in elements)
                {
                    var names = element.GetChildNames(searchStrategy);

                    var expectedNotFound = requiredNames.Where(rn => !names.Contains(rn)).ToList();//get a list of all required names not found
                    var foundNotExpected = names.Where(n => !requiredNames.Contains(n)).ToList();//get a list of all found names that weren't required

                    if (strict && names.Count() != requiredNames.Count)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo(
                            expectedNotFound.Any() ? "\n" +
                                expectedNotFound.Aggregate((a, b) => a + ", " + b) +
                                (expectedNotFound.Count() > 1 ?
                                    " were expected as names but not found. " :
                                    " was expected as a name but not found. ")
                                : "");
                        result.AddInfo(
                            foundNotExpected.Any() ? "\n" +
                                foundNotExpected.Aggregate((a, b) => a + ", " + b) +
                                (foundNotExpected.Count() > 1 ?
                                    " were found as names but not expected. " :
                                    " was found as a name but not expected. ")
                                : "");
                    }
                }
            };
        }

        /// <summary>
        /// Check that the clear button can be tabbed to and that it can be activated with space
        /// </summary>
        /// <returns></returns>
        public static Action<IEnumerable<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckClearButton()
        {
            return (elements, driver, ids, result) =>
            {
                Func<string, string> inputValue = (id) => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                Action<string> clearInput = (id) => driver.ExecuteScript("document.getElementById('" + id + "').value = ''", 0);

                foreach (var id in ids.Take(1))
                {
                    //Enter something, tab to the clear button, clear with space
                    driver.SendKeys(id, "x");
                    driver.SendSpecialKeys(id, WebDriverKey.Wait);
                    if (!elements.Any(e => e.GetAllDescendants().Any(d => d.CurrentName.ToLowerInvariant().Contains("clear value"))))
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nUnable to find a clear button as a child of any element");
                    }
                    //Don't leave input which could cause problems with other tests
                    clearInput(id);
                }
            };
        }

        /// <summary>
        /// Wait for a condition that tests a double value
        /// </summary>
        /// <param name="conditionCheck">The condition checker</param>
        /// <param name="value">The double value to look for</param>
        /// <returns>true if the condition passes, false otherwise</returns>
        public static bool WaitForCondition(Func<double, bool> conditionCheck, double value, int attempts = 20)
        {
            for (var i = 0; i < attempts; i++)
            {
                Thread.Sleep(500);
                if (conditionCheck(value))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Wait for a condition that tests a double value
        /// </summary>
        /// <param name="conditionCheck">The condition checker</param>
        /// <param name="reverse">Whether to reverse the result</param>
        /// <param name="attempts">How many times to check</param>
        /// <returns>true if the condition passes, false otherwise</returns>
        public static bool WaitForCondition(Func<bool> conditionCheck, Action waitAction = null, bool reverse = false, int attempts = 20)
        {
            for (var i = 0; i < attempts; i++)
            {
                Thread.Sleep(500);
                if (reverse ? !conditionCheck() : conditionCheck())
                {
                    return true;
                }
                waitAction?.Invoke();
            }
            return false;
        }
    }
}
