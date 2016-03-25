using Interop.UIAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Edge.A11y
{

    /// <summary>
    /// This is where the logic of the tests is stored
    /// </summary>
    class TestData
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
                    additionalRequirement: ((elements, driver, ids) => CheckChildNames(
                        new List<string> {
                                "Play",
                                "Time elapsed",
                                "Seek",
                                "Time remaining",
                                "Mute",
                                "Volume"})(elements, driver, ids) &&
                        CheckAudioKeyboardInteractions(elements, driver, ids))),//TODO get full list when it's decided
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
                new TestData("input-date", "Calendar", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: CheckCalendarKeyboard(3)),
                new TestData("input-datetime-local", "Calendar"),
                new TestData("input-email", "Edit", "email", keyboardElements: new List<string> { "input1", "input2" }, additionalRequirement: CheckValidation()),
                new TestData("input-month", "Calendar", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: CheckCalendarKeyboard(2)),
                new TestData("input-number", "Edit", "number", keyboardElements: new List<string> { "input1", "input2" }, additionalRequirement: CheckValidation()),
                new TestData("input-range", "Slider", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: (elements, driver, ids) => ids.All(id => {
                        Func<int> RangeValue = () => (int) Int32.Parse((string) driver.ExecuteScript("return document.getElementById('" + id + "').value", 0));

                        var initial = RangeValue();
                        driver.SendSpecialKeys(id, "Arrow_up");
                        if (initial >= RangeValue())
                        {
                            return false;
                        }
                        driver.SendSpecialKeys(id, "Arrow_down");
                        if (initial != RangeValue())
                        {
                            return false;
                        }

                        driver.SendSpecialKeys(id, "Arrow_right");
                        if (initial >= RangeValue())
                        {
                            return false;
                        }
                        driver.SendSpecialKeys(id, "Arrow_left");
                        if (initial != RangeValue())
                        {
                            return false;
                        }

                        return true;
                    })),
                new TestData("input-search", "Edit", "search", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("input-tel", "Edit", "telephone", keyboardElements: new List<string> { "input1", "input2" }),
                new TestData("input-time", "Spinner", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: CheckCalendarKeyboard(3)),
                new TestData("input-url", "Edit", "url", keyboardElements: new List<string> { "input1", "input2" }, additionalRequirement: CheckValidation()),
                new TestData("input-week", "Calendar", keyboardElements: new List<string> { "input1", "input2" },
                    additionalRequirement: CheckCalendarKeyboard(2)),
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
                    additionalRequirement: ((elements, driver, ids) =>
                        CheckChildNames(
                            new List<string> {//TODO get full list when it's decided
                                    "Play",
                                    "Time elapsed",
                                    "Seek",
                                    "Time remaining",
                                    "Zoom in",
                                    "Show audio",
                                    "Show captioning",
                                    "Mute",
                                    "Volume",
                                    "Full screen" })(elements, driver, ids) &&
                        CheckVideoKeyboardInteractions(elements, driver, ids))),
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
        /// Checks video elements with the requirements outlined by GWhit
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="driver"></param>
        /// <param name="ids"></param>
        /// <returns>True if the element passes, false othersie</returns>
        private static bool CheckVideoKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids)
        {
            string videoId = "video1";
            Func<bool> VideoPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + videoId + "').paused", 0);
            Func<double> VideoVolume = () => (double)driver.ExecuteScript("return document.getElementById('" + videoId + "').volume", 0);
            Func<bool> VideoMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').muted", 0);
            Func<double> VideoElapsed = () => (double)driver.ExecuteScript("return document.getElementById('" + videoId + "').currentTime", 0);

            //Case 1: tab to video element and play/pause
            driver.SendSpecialKeys(videoId, "Space");
            if (!VideoPlaying())
            {
                return false;
            }
            driver.SendSpecialKeys(videoId, "Space");
            if (VideoPlaying())
            {
                return false;
            }

            //Case 2: tab to play button and play/pause
            driver.SendSpecialKeys(videoId, "TabSpace");
            if (!VideoPlaying())
            {
                return false;
            }
            driver.SendSpecialKeys(videoId, "Enter");
            if (VideoPlaying())
            {
                return false;
            }

            //TODO remove when the test file is resized
            driver.ExecuteScript("document.getElementById('video1').width = 600", 0);

            //Case 3: Volume and mute
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 6);//tab to volume control //TODO make this more resilient to UI changes
            var initial = VideoVolume();
            driver.SendSpecialKeys(videoId, "Arrow_downArrow_down");//volume up
            if (initial == VideoVolume())
            {
                return false;
            }
            driver.SendSpecialKeys(videoId, "Arrow_upArrow_up");//volume down
            if (VideoVolume() != initial)
            {
                return false;
            }
            driver.SendSpecialKeys(videoId, "Enter");//mute//TODO switch back to original order once space works
            if (!VideoMuted())
            {
                return false;
            }
            driver.SendSpecialKeys(videoId, "Space");//unmute
            if (VideoMuted())
            {
                return false;
            }

            //Case 4: Audio selection
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 5);//tab to audio selection//TODO make this more resilient to UI changes
            driver.SendSpecialKeys(videoId, "EnterArrow_up");

            //Case 5: Progress and seek
            if (VideoPlaying())
            { //this should not be playing
                return false;
            }
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 3);//tab to seek//TODO make this more resilient to UI changes
            initial = VideoElapsed();
            driver.SendSpecialKeys(videoId, "Arrow_right"); //skip ahead
            if (initial != VideoElapsed() - 10)
            {
                return false;
            }

            driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
            if (initial != VideoElapsed())
            {
                return false;
            }

            //Case 6: Progress and seek on remaining time
            if (VideoPlaying())
            { //this should not be playing
                return false;
            }
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 4);//tab to seek//TODO make this more resilient to UI changes
            initial = VideoElapsed();
            driver.SendSpecialKeys(videoId, "Arrow_right"); //skip ahead
            if (initial != VideoElapsed() - 10)
            {
                return false;
            }

            driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
            if (initial != VideoElapsed())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks audio elements with the requirements outlined by GWhit
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="driver"></param>
        /// <param name="ids"></param>
        /// <returns>True if the element passes, false othersie</returns>
        private static bool CheckAudioKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids)
        {
            string audioId = "audio1";
            Func<bool> AudioPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + audioId + "').paused", 0);
            Func<double> AudioVolume = () => (double)driver.ExecuteScript("return document.getElementById('" + audioId + "').volume", 0);
            Func<bool> AudioMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + audioId + "').muted", 0);
            Func<double> AudioElapsed = () => (double)driver.ExecuteScript("return document.getElementById('" + audioId + "').currentTime", 0);

            //Case 1: Play/Pause
            driver.SendTabs(audioId, 1); //Tab to play button
            driver.SendSpecialKeys(audioId, "Enter");
            if (!AudioPlaying())
            {
                return false;
            }

            driver.SendSpecialKeys(audioId, "Space");
            if (AudioPlaying())
            {
                return false;
            }

            //Case 2: Seek
            if (AudioPlaying())
            {
                return false;
            }
            driver.SendTabs(audioId, 3);
            var initial = AudioElapsed();
            driver.SendSpecialKeys(audioId, "Arrow_right");
            if (initial == AudioElapsed())
            {
                return false;
            }
            driver.SendSpecialKeys(audioId, "Arrow_left");
            if (initial != AudioElapsed())
            {
                return false;
            }

            //Case 3: Volume and mute
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(audioId, 5);
            initial = AudioVolume();
            driver.SendSpecialKeys(audioId, "Arrow_down");
            if (initial == AudioVolume())
            {
                return false;
            }

            driver.SendSpecialKeys(audioId, "Arrow_up");
            if (initial != AudioVolume())
            {
                return false;
            }

            driver.SendSpecialKeys(audioId, "Space");
            if (!AudioMuted())
            {
                return false;
            }

            driver.SendSpecialKeys(audioId, "Enter");
            if (AudioMuted())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generic func factory for all the calendar elements
        /// </summary>
        /// <param name="fields">A count of how many fields there are in the calendar element</param>
        /// <returns>A func that tests the element</returns>
        public static Func<List<IUIAutomationElement>, DriverManager, List<string>, bool> CheckCalendarKeyboard(int fields)
        {
            return new Func<List<IUIAutomationElement>, DriverManager, List<string>, bool>((elements, driver, ids) =>
                ids.All(id =>
                {
                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);

                    //Open the calendar and close it without changing anything
                    var initial = DateValue();
                    driver.SendSpecialKeys(id, "EnterEscape");
                    if (initial != DateValue())
                    {
                        return false;
                    }

                    //Accept the default value
                    driver.SendSpecialKeys(id, "EnterEnter");
                    var today = DateValue();
                    if (today == initial)
                    {
                        return false;
                    }

                    //Open the menu
                    driver.SendSpecialKeys(id, "Enter");
                    //Change each field in the calendar
                    for (int i = 0; i < fields; i++)
                    {
                        driver.SendSpecialKeys(id, "Arrow_downTab");
                    }
                    //Get the altered value, which should be one less than the default
                    //for each field
                    var newdate = DateValue();
                    var newdatesplit = newdate.Split('-');
                    var initialsplit = initial.Split('-');

                    //ensure that all fields have been changed
                    for (int i = 0; i < fields; i++)
                    {
                        if (newdatesplit[i] == initialsplit[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }));
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
