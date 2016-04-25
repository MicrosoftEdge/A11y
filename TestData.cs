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
        public const string ARFAIL = "Failed additional requirement";
        public const string ARPASS = "";
        /// <summary>
        /// The name of the test, which corresponds to the url of the page to test
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
            return new List<TestData>{
                new TestData("sample", "Combobox", null, keyboardElements: new List<string> { "video1" },
                    additionalRequirement: CheckVideoKeyboardInteractions)
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
            Func<bool> VideoPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + videoId + "').paused", 0);
            Func<double> VideoVolume = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').volume", 0).ParseMystery();
            Func<bool> VideoMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').muted", 0);
            Func<double> VideoElapsed = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').currentTime", 0).ParseMystery();

            //Case 1: tab to video element and play/pause
            driver.SendSpecialKeys(videoId, "Space");
            if (!VideoPlaying())
            {
                return "Video was not playing after spacebar on root element";
            }
            driver.SendSpecialKeys(videoId, "Space");
            if (VideoPlaying())
            {
                return "Video was not paused after spacebar on root element";
            }

            //Case 2: tab to play button and play/pause
            driver.SendSpecialKeys(videoId, "TabSpace");
            if (!VideoPlaying())
            {
                return "Video was not playing after spacebar on play button";
            }
            driver.SendSpecialKeys(videoId, "Enter");
            if (VideoPlaying())
            {
                return "Video was not paused after enter on play button";
            }

            //TODO remove when the test file is resized
            driver.ExecuteScript("document.getElementById('video1').width = 600", 0);

            //Case 3: Volume and mute
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 6);//tab to volume control //TODO make this more resilient to UI changes
            var initial = VideoVolume();
            driver.SendSpecialKeys(videoId, "Arrow_downArrow_down");//volume down
            if (initial == VideoVolume())
            {
                return "Volume did not decrease with arrow keys";
            }
            driver.SendSpecialKeys(videoId, "Arrow_upArrow_up");//volume up
            if (VideoVolume() != initial)
            {
                return "Volume did not increase with arrow keys";
            }
            driver.SendSpecialKeys(videoId, "Enter");//mute//TODO switch back to original order once space works
            if (!VideoMuted())
            {
                return "Enter did not mute the video";
            }
            driver.SendSpecialKeys(videoId, "Space");//unmute
            if (VideoMuted())
            {
                return "Space did not unmute the video";
            }

            //Case 4: Audio selection
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 5);//tab to audio selection//TODO make this more resilient to UI changes
            driver.SendSpecialKeys(videoId, "EnterArrow_up");

            //Case 5: Progress and seek
            if (VideoPlaying())
            { //this should not be playing
                return "Video was playing when it shouldn't have been";
            }
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 3);//tab to seek//TODO make this more resilient to UI changes
            initial = VideoElapsed();
            driver.SendSpecialKeys(videoId, "Arrow_right"); //skip ahead
            if (initial != VideoElapsed() - 10)
            {
                return "Video did not skip forward with arrow right";
            }

            driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
            if (initial != VideoElapsed())
            {
                return "Video did not skip back with arrow left";
            }

            //Case 6: Progress and seek on remaining time
            if (VideoPlaying())
            { //this should not be playing
                return "Video was playing when it shouldn't have been";
            }
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(videoId, 4);//tab to seek//TODO make this more resilient to UI changes
            initial = VideoElapsed();
            driver.SendSpecialKeys(videoId, "Arrow_right"); //skip ahead
            if (initial != VideoElapsed() - 10)
            {
                return "Video did not skip forward with arrow right";
            }

            driver.SendSpecialKeys(videoId, "Arrow_left"); //skip back
            if (initial != VideoElapsed())
            {
                return "Video did not skip back with arrow left";
            }

            return ARPASS;
        }
    }
}
