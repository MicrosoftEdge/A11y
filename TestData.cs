using Interop.UIAutomationCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.Edge.A11y.ElementConverter;

namespace Microsoft.Edge.A11y
{
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

        private List<UIAPattern> _RequiredPatterns;
        public List<UIAPattern> RequiredPatterns
        {
            get
            {
                return _RequiredPatterns;
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
        public TestData(string testName,
            UIAControlType controlType,
            string localizedControlType = null,
            UIALandmarkType landmarkType = UIALandmarkType.Unknown,
            string localizedLandmarkType = null,
            List<UIAPattern> requiredPatterns = null)
        {
            _TestName = testName;
            _ControlType = controlType;
            _LocalizedControlType = localizedControlType;
            _LandmarkType = landmarkType;
            _LocalizedLandmarkType = localizedLandmarkType;
            _RequiredPatterns = requiredPatterns;
        }

        /// <summary>
        /// Convert an element into a pattern interface
        /// </summary>
        /// <typeparam name="T">The pattern interface to implement</typeparam>
        /// <param name="patternName">The friendly name of the pattern</param>
        /// <param name="element">The element to convert</param>
        /// <returns>The element as an implementation of the pattern</returns>
        private static T GetPattern<T>(UIAPattern patternName, IUIAutomationElement element)
        {
            List<UIAPattern> patternNames;
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
            var element = parent.GetAllDescendents(e => e.CurrentName.Contains(name)).First();
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
