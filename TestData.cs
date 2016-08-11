namespace Microsoft.Edge.A11y
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using static ElementConverter;
    using Interop.UIAutomationCore;

    /// <summary>
    /// This is where the logic of the tests is stored
    /// </summary>
    internal class TestData
    {
        /// <summary>
        /// Epsilon value used for double comparison
        /// </summary>
        public const double Epsilon = .001;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestData"/> class
        /// </summary>
        /// <param name="testName">The name of the test</param>
        /// <param name="controlType">The control type of the test</param>
        /// <param name="localizedControlType">The localized control type of the test</param>
        /// <param name="landmarkType">The landmark type of the test</param>
        /// <param name="localizedLandmarkType">The localized landmark type of the test</param>
        /// <param name="keyboardElements">The keyboard-accessible elements to look for</param>
        /// <param name="searchStrategy">The search strategy to use when looking for an element</param>
        /// <param name="requiredNames">The required names on the element's web page</param>
        /// <param name="requiredDescriptions">The required descriptions on the element's web page</param>
        /// <param name="additionalRequirement">The additional requirement for the test</param>
        public TestData(
            string testName,
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
            this.TestName = testName;
            this.ControlType = controlType;
            this.LocalizedControlType = localizedControlType;
            this.LandmarkType = landmarkType;
            this.LocalizedLandmarkType = localizedLandmarkType;
            this.KeyboardElements = keyboardElements;
            this.SearchStrategy = searchStrategy;
            this.RequiredNames = requiredNames;
            this.RequiredDescriptions = requiredDescriptions;
            this.AdditionalRequirement = additionalRequirement;
        }

        /// <summary>
        /// Gets the name of the test, which corresponds to the name of the html element
        /// </summary>
        public string TestName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the UIA control type we will use to search for the element
        /// </summary>
        public UIAControlType ControlType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the UIA localized control type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string LocalizedControlType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the UIA landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public UIALandmarkType LandmarkType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the UIA localized landmark type, which will be part of the test
        /// case if it is not null
        /// </summary>
        public string LocalizedLandmarkType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of ids for all the elements that should be keyboard accessible (via tab)
        /// </summary>
        public List<string> KeyboardElements
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets if not null, this func will be used to test elements to see if they should be
        /// tested (instead of matching _ControlType).
        /// </summary>
        public Func<IUIAutomationElement, bool> SearchStrategy
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a list of expected values which will be compared to the accessible names of
        /// the found elements.
        /// </summary>
        public List<string> RequiredNames
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets same as above, but for accessible descriptions.
        /// </summary>
        public List<string> RequiredDescriptions
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a func that allows extending the tests for specific elements. 
        /// </summary>
        public Func<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> AdditionalRequirement
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates the list of tests for html5accessibility.com    
        /// </summary>
        /// <returns>Returns the list of tests</returns>
        public static List<TestData> AllTests()
        {
            const int Timeout = 0;
            var uia = new CUIAutomation8();
            var walker = uia.RawViewWalker;

            return new List<TestData>
            {
                new TestData(
                    "article",
                    UIAControlType.Group,
                    "article",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "h1 referenced by aria-describedby6",
                            "title attribute 7"
                        }),
                new TestData(
                    "aside",
                    UIAControlType.Group,
                    "aside",
                    UIALandmarkType.Custom,
                    "complementary",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "h1 referenced by aria-describedby6",
                            "title attribute 7"
                        }),
                new TestData(
                    "audio",
                    UIAControlType.Group,
                    "audio",
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        CheckChildNames(
                            new List<string>
                            {
                                "Play",
                                "Time elapsed/Skip back",
                                "Seek",
                                "Time remaining/Skip ahead",
                                "Mute",
                                "Volume"
                            })(elements, driver, ids, result);

                            CheckAudioKeyboardInteractions(elements, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "canvas",
                    UIAControlType.Image,
                    additionalRequirement: 
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            var subdomElements = new List<IUIAutomationElement>();

                            foreach (var e in elements)
                            {
                                subdomElements.AddRange(e.GetAllDescendants((d) =>
                                {
                                    var convertedRole = GetControlTypeFromCode(d.CurrentControlType);
                                    return convertedRole == UIAControlType.Button || convertedRole == UIAControlType.Text;
                                }));
                            }

                            if (subdomElements.Count != 3)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("Unable to find subdom elements");
                            }

                            const string featureDetectionScript = @"canvas = document.getElementById('myCanvas');
                                                                    isSupported = !!(canvas.getContext && canvas.getContext('2d'));
                                                                    isSupported = isSupported && !!(canvas.getContext('2d').drawFocusIfNeeded);
                                                                    return isSupported;";

                            if (!(bool)driver.ExecuteScript(featureDetectionScript, Timeout))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nFailed feature detection");
                            }

                            return result;
                        }),
                new TestData(
                    "datalist",
                    UIAControlType.Combobox,
                    keyboardElements:
                        new List<string>
                        {
                            "input1"
                        },
                    additionalRequirement:
                    (elements, driver, ids) =>
                    {
                        Func<string, string> datalistValue = (id) => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                        var result = new TestCaseResultExt();

                        foreach (var element in elements)
                        {
                            var elementFive = (IUIAutomationElement5)element;
                            List<int> patternIds;
                            var names = elementFive.GetPatterns(out patternIds);

                            if (!names.Contains("SelectionPattern"))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not support SelectionPattern");
                            }
                            else
                            {
                                var selectionPattern = (IUIAutomationSelectionPattern)elementFive.GetCurrentPattern(
                                    patternIds[names.IndexOf("SelectionPattern")]);

                                if (selectionPattern.CurrentCanSelectMultiple == 1)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nCanSelectMultiple set to true");
                                }
                            }
                        }

                        var previousControllerForElements = new HashSet<int>();

                        // keyboard a11y
                        foreach (var id in ids.Take(1))
                        {
                            var initial = datalistValue(id);
                            driver.SendSpecialKey(id, WebDriverKey.Arrow_down);

                            var controllerForElements = elements
                                .Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0)
                                .Select(element => elements.IndexOf(element))
                                .ToList();
                            if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("Element controller for not set for id: " + id);
                            }

                            previousControllerForElements.Add(controllerForElements.First(element => !previousControllerForElements.Contains(element)));

                            driver.SendSpecialKey(id, WebDriverKey.Enter);
                            if (datalistValue(id) != "Item value 1")
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("Unable to set the datalist with keyboard for element with id: " + id);
                                return result;
                            }
                        }

                        return result;
                    }),
                new TestData(
                    "figure",
                    UIAControlType.Group,
                    "figure",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "title attribute 4",
                            "Figcaption element 5",
                            "Figcaption element 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        }),

                // Control type is ignored, verify this element via text range
                new TestData(
                    "figure-figcaption",
                    UIAControlType.Text,
                    searchStrategy: element => true,
                    additionalRequirement: (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            const string logoText = "HTML5 logo 1";

                            // there will be only one, since element is the pane in this case
                            foreach (var element in elements)
                            {
                                var five = (IUIAutomationElement5)walker.GetFirstChildElement(walker.GetFirstChildElement(elements[0]));

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

                                HashSet<UIAControlType> foundControlTypes;
                                var figure = EdgeA11yTools.SearchChildren(element, UIAControlType.Group, null, out foundControlTypes);

                                var childRange = textPattern.RangeFromChild(figure[0]);

                                var childRangeText = childRange.GetText(1000).Trim();

                                if (childRangeText != logoText)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo(string.Format("\nUnable to find correct text range. Found '{0}' instead", childRangeText));
                                }
                            }

                            return result;
                        }),
                new TestData(
                    "footer",
                    UIAControlType.Group,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Group
                        && element.CurrentLocalizedControlType != "article",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 3",
                            "small referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "small referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement: (elements, driver, ids) => 
                    {
                        var result = new TestCaseResultExt();

                        if (elements.Count != 7)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + elements.Count + " elements, expected 7.");
                        }

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;

                        // same for landmark and localizedlandmark
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
                new TestData(
                    "header",
                    UIAControlType.Group,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Group
                        && element.CurrentLocalizedControlType != "article",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "h1 referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        if (elements.Count != 7)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + elements.Count + " elements, expected 7.");
                        }

                        var convertedLandmarks = 0;
                        var localizedLandmarks = 0;
                        var landmarkCode = 0;

                        // same for landmark and localizedlandmark
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
                new TestData(
                    "input-color",
                    UIAControlType.Button,
                    "color picker",
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        var previousControllerForElements = new HashSet<int>();
                        foreach (var id in ids.Take(1))
                        {
                            Func<string> CheckColorValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", Timeout);
                            Action<string> ChangeColorValue = (value) => driver.ExecuteScript("document.getElementById('" + id + "').value = '" + value + "'", Timeout);
                            Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                            var initial = CheckColorValue();
                            driver.SendSpecialKeys(
                                id,
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Enter,
                                    WebDriverKey.Escape,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Enter
                                });

                            // open dialog to check controllerfor
                            driver.SendSpecialKey(id, WebDriverKey.Enter);
                            var controllerForElements = elements
                                .Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0)
                                .Select(element => elements.IndexOf(element))
                                .ToList();
                            if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement controller for not set for id: " + id);
                            }
                            else
                            {
                                // the element that corresponds to this id
                                var thisElement = elements[controllerForElements.First(element => !previousControllerForElements.Contains(element))];
                                if (thisElement.CurrentControllerFor.Length > 1)
                                {
                                    throw new Exception("\nMore than one ControllerFor present, test assumption failed");
                                }

                                var thisDialog = thisElement.CurrentControllerFor.GetElement(0);
                                var descendents = thisDialog.GetAllDescendants();

                                // sliders
                                var sliders = descendents
                                    .Where(d => GetControlTypeFromCode(d.CurrentControlType) == UIAControlType.Slider)
                                    .ToList();
                                if (sliders.Count != 3)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nDialog did not have three slider elements");
                                }
                                else if (!sliders.All(s => s.GetPatterns().Contains("RangeValuePattern")))
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nDialog's sliders did not implement RangeValuePattern");
                                }

                                // buttons
                                if (descendents.Count(d => GetControlTypeFromCode(d.CurrentControlType) == UIAControlType.Button) != 2)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nDialog did not have two button elements");
                                }

                                // color well
                                var outputs = descendents
                                    .Where(d => GetControlTypeFromCode(d.CurrentControlType) == UIAControlType.Group && d.CurrentLocalizedControlType == "output")
                                    .ToList();
                                if (outputs.Count > 1)
                                {
                                    throw new Exception("Test assumption failed: expected color dialog to have at most one output");
                                }
                                else if (outputs.Count == 0)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nCould not find output in color dialog");
                                }
                                else if (outputs.Count == 1)
                                {
                                    var output = outputs.First();
                                    if (string.IsNullOrEmpty(output.CurrentName))
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nColor dialog output did not have name set");
                                    }
                                    else
                                    {
                                        var initialName = output.CurrentName;

                                        driver.SendSpecialKeys(
                                            id,
                                            new List<WebDriverKey>
                                            {
                                                WebDriverKey.Tab,
                                                WebDriverKey.Tab,
                                                WebDriverKey.Arrow_right,
                                                WebDriverKey.Arrow_right
                                            });

                                        if (output.CurrentName == initialName)
                                        {
                                            result.Result = ResultType.Half;
                                            result.AddInfo("\nColor dialog output did not change name when color changed: " + output.CurrentName);
                                        }

                                        // Ensure that the color is clear before continuing
                                        ChangeColorValue("#000000");
                                    }
                                }
                            }

                            // open with enter, close with escape
                            driver.SendSpecialKeys(
                                id,
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Escape,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Escape
                                });
                            if (CheckColorValue() != initial)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to cancel with escape");
                            }

                            // open with enter, close with enter
                            driver.SendSpecialKeys(
                                id,
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Escape,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Enter
                                });
                            if (CheckColorValue() == initial)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to change value with arrow keys and submit with enter");
                            }

                            // open with space, close with enter
                            initial = CheckColorValue();
                            driver.SendSpecialKeys(
                                id,
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Escape,
                                    WebDriverKey.Space,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Arrow_right,
                                    WebDriverKey.Enter
                                });
                            if (initial == CheckColorValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to open dialog with space");
                            }

                            initial = CheckColorValue();

                            driver.SendSpecialKeys(
                                id,
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Enter,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Tab
                                });
                            if (ActiveElement() != id)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to reach accept/dismiss buttons via tab");
                            }

                            // only try to use the buttons if they're there
                            else
                            {
                                // **Dismiss button**
                                // Open the dialog, change hue, tab to cancel button, activate it with space,
                                // check that tabbing moves to the previous button (on the page not the dialog)
                                driver.SendSpecialKeys(
                                    id,
                                    new List<WebDriverKey>
                                    {
                                        WebDriverKey.Escape,
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
                                        WebDriverKey.Shift
                                    });
                                if (initial != CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to cancel with dismiss button via space");
                                }

                                // do the same as above, but activate the button with enter this time
                                driver.SendSpecialKeys(
                                    id,
                                    new List<WebDriverKey>
                                    {
                                        WebDriverKey.Escape,
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
                                        WebDriverKey.Shift
                                    });
                                if (initial != CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to cancel with dismiss button via enter");
                                }

                                // **Accept button**
                                initial = CheckColorValue();

                                // Open the dialog, tab to hue, change hue, tab to accept button, activate it with space,
                                // send tab (since the dialog should be closed, this will transfer focus to the next
                                // input-color button)
                                driver.SendSpecialKeys(
                                    id,
                                    new List<WebDriverKey>
                                    {
                                        WebDriverKey.Escape,
                                        WebDriverKey.Enter,
                                        WebDriverKey.Tab,
                                        WebDriverKey.Tab,
                                        WebDriverKey.Arrow_right,
                                        WebDriverKey.Arrow_right,
                                        WebDriverKey.Tab,
                                        WebDriverKey.Space,
                                        WebDriverKey.Tab
                                    });
                                if (initial == CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to accept with accept button via space");
                                }

                                // the value hopefully changed above, but just to be safe
                                initial = CheckColorValue();

                                // Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                                // We don't have to worry about why the dialog closed here (button or global enter)
                                driver.SendSpecialKeys(
                                    id,
                                    new List<WebDriverKey>
                                    {
                                        WebDriverKey.Escape,
                                        WebDriverKey.Enter,
                                        WebDriverKey.Tab,
                                        WebDriverKey.Tab,
                                        WebDriverKey.Arrow_right,
                                        WebDriverKey.Arrow_right,
                                        WebDriverKey.Tab,
                                        WebDriverKey.Enter,
                                        WebDriverKey.Tab
                                    });
                                if (initial == CheckColorValue() || ActiveElement() == id)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nUnable to accept with accept button via enter");
                                }
                            }
                        }

                        return result;
                    }),
                new TestData(
                    "input-date",
                    UIAControlType.Edit,
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (element, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckCalendar(3)(element, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "input-datetime-local",
                    UIAControlType.Edit,
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (element, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckDatetimeLocal()(element, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "input-email",
                    UIAControlType.Edit,
                    "email",
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();
                        CheckValidation()(elements, driver, ids, result);
                        CheckClearButton()(elements, driver, ids, result);
                        return result;
                    }),
                new TestData(
                    "input-month",
                    UIAControlType.Edit,
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckCalendar(2)(elements, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "input-number",
                    UIAControlType.Spinner,
                    "number",
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckValidation()(elements, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "input-range",
                    UIAControlType.Slider,
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        // keyboard interaction
                        foreach (var id in ids.Take(1))
                        {
                            Func<int> RangeValue = () => int.Parse((string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0));

                            var initial = RangeValue();
                            driver.SendSpecialKey(id, WebDriverKey.Arrow_up);
                            if (initial == RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to increase range with arrow up");
                                continue;
                            }

                            driver.SendSpecialKey(id, WebDriverKey.Arrow_down);
                            if (initial != RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to decrease range with arrow down");
                                continue;
                            }

                            driver.SendSpecialKey(id, WebDriverKey.Arrow_right);
                            if (initial >= RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to increase range with arrow right");
                                continue;
                            }

                            driver.SendSpecialKey(id, WebDriverKey.Arrow_left);
                            if (initial != RangeValue())
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nUnable to decrease range with arrow left");
                                continue;
                            }
                        }

                        // rangevalue pattern
                        foreach (var element in elements)
                        {
                            if (!element.GetPatterns().Contains("RangeValuePattern"))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not implement the RangeValuePattern");
                            }
                        }

                        return result;
                    }),
                new TestData(
                    "input-search",
                    UIAControlType.Edit,
                    "search",
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckClearButton()(elements, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "input-tel",
                    UIAControlType.Edit,
                    "telephone",
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckClearButton()(elements, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "input-time",
                    UIAControlType.Edit,
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckCalendar(3, 2)(elements, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "input-url",
                    UIAControlType.Edit,
                    "url",
                        keyboardElements:
                            new List<string>
                            {
                                "input1",
                                "input2"
                            },
                        additionalRequirement:
                            (elements, driver, ids) =>
                            {
                                var result = new TestCaseResultExt();
                                CheckValidation()(elements, driver, ids, result);
                                CheckClearButton()(elements, driver, ids, result);
                                return result;
                            }),
                new TestData(
                    "input-week",
                    UIAControlType.Edit,
                    keyboardElements:
                        new List<string>
                        {
                            "input1",
                            "input2"
                        },
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping input 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckCalendar(2)(elements, driver, ids, result);
                            return result;
                        }),
                new TestData(
                    "main",
                    UIAControlType.Group,
                    "main",
                    UIALandmarkType.Main,
                    "main",
                    requiredNames:
                        new List<string>
                        {
                            "title attribute 1",
                            "aria-label attribute 2",
                            "h1 referenced by aria-labelledby3",
                            "title attribute 4",
                            "aria-label attribute 6"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "h1 referenced by aria-describedby5",
                            "title attribute 6"
                        }),
                new TestData(
                    "mark",
                    UIAControlType.Text,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Text
                        && element.CurrentLocalizedControlType == "mark",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute2",
                            "Element referenced by aria-labelledby attribute3",
                            "title attribute 4",
                            "aria-label attribute 6"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "Element referenced by aria-describedby attribute5",
                            "title attribute 6"
                        }),
                new TestData(
                    "meter",
                    UIAControlType.Progressbar,
                    "meter",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping meter 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();

                            // readonly
                            if (!elements.All(element => element.GetProperties().Any(p => p.Contains("IsReadOnly"))))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("Not all elements were read only");
                            }

                            // rangevalue
                            foreach (var element in elements)
                            {
                                const string patternName = "RangeValuePattern";

                                var patterned = GetPattern<IUIAutomationRangeValuePattern>(patternName, element);
                                if (patterned == null)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not support " + patternName);
                                }
                                else
                                {
                                    if (patterned.CurrentMaximum - 100 > Epsilon)
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nElement did not have the correct max");
                                    }

                                    if (patterned.CurrentMinimum - 0 > Epsilon)
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nElement did not have the correct min");
                                    }

                                    // All the meters are set to this
                                    const double value = 83.5;
                                    if (patterned.CurrentValue - value > Epsilon)
                                    {
                                        result.Result = ResultType.Half;
                                        result.AddInfo("\nElement did not have the correct value");
                                    }
                                }
                            }

                            // value
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
                        },
                    searchStrategy:
                        element => element.GetPatterns().Contains("RangeValuePattern")),
                new TestData(
                    "nav",
                    UIAControlType.Group,
                    "navigation",
                    UIALandmarkType.Navigation,
                    "navigation",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "h1 referenced by aria-labelledby3",
                            "title attribute 4",
                            "aria-label attribute 6"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "h1 referenced by aria-describedby5",
                            "title attribute 6"
                        }),
                new TestData(
                    "output",
                    UIAControlType.Group,
                    "output",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping output 410",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        if (!elements.All(element => ((IUIAutomationElement5)element).CurrentLiveSetting == LiveSetting.Polite))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have LiveSetting = Polite");
                        }

                        var controllerForLengths = elements.Select(element => element.CurrentControllerFor != null ? element.CurrentControllerFor.Length : 0);
                        var positiveLengthCount = controllerForLengths.Count(cfl => cfl > 0);
                        if (positiveLengthCount != 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nExpected 1 element with ControllerFor set. Found " + positiveLengthCount);
                        }

                        return result;
                    }),
                new TestData(
                    "progress",
                    UIAControlType.Progressbar,
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 2",
                            "p referenced by aria-labelledby3",
                            "label wrapping output 4",
                            "title attribute 5",
                            "label referenced by for/id attributes 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "p referenced by aria-describedby6",
                            "title attribute 7"
                        },
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        // range value
                        foreach (var element in elements)
                        {
                            const string patternName = "RangeValuePattern";

                            var patterned = GetPattern<IUIAutomationRangeValuePattern>(patternName, element);
                            if (patterned == null)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not support " + patternName);
                            }
                            else
                            {
                                if (patterned.CurrentMaximum - 100 > Epsilon)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not have the correct max");
                                }

                                if (patterned.CurrentMinimum - 0 > Epsilon)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not have the correct min");
                                }

                                // All the progress bars are set to this
                                const double value = 22;
                                if (patterned.CurrentValue - value > Epsilon)
                                {
                                    result.Result = ResultType.Half;
                                    result.AddInfo("\nElement did not have the correct value");
                                }
                            }
                        }

                        return result;
                    }),
                new TestData(
                    "section",
                    UIAControlType.Group,
                    "section",
                    UIALandmarkType.Custom,
                    "region",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute 3",
                            "h1 referenced by aria-labelledby4",
                            "title attribute 5",
                            "aria-label attribute 7"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "h1 referenced by aria-describedby6",
                            "title attribute 7"
                        }),
                new TestData(
                    "time",
                    UIAControlType.Text,
                    searchStrategy: element =>
                        GetControlTypeFromCode(element.CurrentControlType) == UIAControlType.Text
                        && element.CurrentLocalizedControlType == "time",
                    requiredNames:
                        new List<string>
                        {
                            "aria-label attribute2",
                            "Element referenced by aria-labelledby attribute 3",
                            "title attribute 4",
                            "aria-label attribute 6"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
                            "2015-10-01",
                            "2015-10-02",
                            "2015-10-03",
                            "2015-10-04",
                            "Element referenced by aria-describedby attribute",
                            "title attribute 6",
                        }),
                new TestData(
                    "track",
                    UIAControlType.Unknown,
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();
                        driver.ExecuteScript(Javascript.Track, Timeout);

                        if (!(bool)driver.ExecuteScript("return Modernizr.track && Modernizr.texttrackapi", Timeout))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("Element was not found to be supported by Modernizr");
                        }

                        return result;
                    },
                    searchStrategy:
                        element => true),
                new TestData(
                    "video",
                    UIAControlType.Group,
                    null,
                    keyboardElements:
                        new List<string>
                        {
                            "video1"
                        },
                    additionalRequirement:
                        (elements, driver, ids) =>
                        {
                            var result = new TestCaseResultExt();
                            CheckChildNames(
                                new List<string>
                                {
                                    "Play",
                                    "Time elapsed/Skip back",
                                    "Seek",
                                    "Time remaining/Skip ahead",
                                    "Zoom in",
                                    "Show audio selection menu",
                                    "Show captioning selection menu",
                                    "Mute",
                                    "Volume",
                                    "Full screen"
                                })(elements, driver, ids, result);
                            CheckVideoKeyboardInteractions(elements, driver, ids, result);

                            return result;
                        }),
                new TestData(
                    "hidden-att",
                    UIAControlType.Button,
                    null,
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();

                        var browserElement = EdgeA11yTools.FindBrowserDocument(0);

                        var count = elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane);
                        if (count != 0)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("Found " + count + " elements. Expected 0");
                            return result;
                        }

                        // Make sure the text isn't showing up on the page

                        // We only have the pane element, take its grandchild
                        var five = (IUIAutomationElement5)
                            walker.GetFirstChildElement(
                                walker.GetFirstChildElement(elements[0]));

                        List<int> patternIds;
                        var names = five.GetPatterns(out patternIds);
                        const string textPatternString = "TextPattern";
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
                        if (foundText.Contains("HiDdEn TeXt"))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound text that should have been hidden");
                        }

                        // remove hidden attribute
                        driver.ExecuteScript(Javascript.RemoveHidden, Timeout);

                        // make sure the button show up now that their parents are not hidden
                        HashSet<UIAControlType> foundControlTypes;
                        elements = EdgeA11yTools.SearchChildren(browserElement, UIAControlType.Button, null, out foundControlTypes);
                        count = elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane);
                        if (count != 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + count + " elements. Expected 1");
                        }

                        // remove aria-hidden attribute
                        driver.ExecuteScript(Javascript.RemoveAriaHidden, Timeout);

                        // both buttons should now be visible, since both aria-hidden and hidden attribute are missing
                        elements = EdgeA11yTools.SearchChildren(browserElement, UIAControlType.Button, null, out foundControlTypes);
                        count = elements.Count(e => GetControlTypeFromCode(e.CurrentControlType) != UIAControlType.Pane);
                        if (count != 2)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nFound " + count + " elements. Expected 2");
                        }

                        return result;
                    },
                    searchStrategy: element => true),
                new TestData(
                    "required-att",
                    UIAControlType.Edit,
                    additionalRequirement: (elements, driver, ids) =>
                    {
                        var result = new TestCaseResultExt();
                        driver.SendSpecialKey("input1", WebDriverKey.Enter);
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));

                        // there can only be one
                        foreach (var element in elements)
                        {
                            if (element.CurrentControllerFor == null || element.CurrentControllerFor.Length == 0)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not have controller for set");
                            }

                            if (element.CurrentIsRequiredForForm != 1)
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not have IsRequiredForForm set to true");
                            }

                            if (string.IsNullOrEmpty(element.CurrentHelpText))
                            {
                                result.Result = ResultType.Half;
                                result.AddInfo("\nElement did not have HelpText");
                            }
                        }

                        return result;
                    }),
                new TestData(
                    "placeholder-att",
                    UIAControlType.Edit,
                    requiredNames:
                        new List<string>
                        {
                            "placeholder text 1",
                            "Label text 2:",
                            "Label text 3:",
                            "placeholder text 4",
                            "placeholder text 5",
                            "aria-placeholder text 6"
                        },
                    requiredDescriptions:
                        new List<string>
                        {
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
            var index = patternNames.IndexOf(patternName);
            if (index < 0)
            {
                return default(T);
            }
            else
            {
                T pattern = (T)((IUIAutomationElement5)element).GetCurrentPattern(patternIds[index]);
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
                driver.SendSpecialKey(tabId, WebDriverKey.Tab);
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
        /// Checks keyboard interactions for the video control
        /// </summary>
        /// <param name="elements">The UIA elements on the page</param>
        /// <param name="driver">The WebDriver wrapper</param>
        /// <param name="ids">The html ids of elements found on the page</param>
        /// <param name="result">The aggregate result of the testing</param>
        private static void CheckVideoKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids, TestCaseResultExt result)
        {
            const string videoId = "video1";

            Func<bool> videoPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + videoId + "').paused", 0);
            Func<object> pauseVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').pause()", 0);
            Func<object> playVideo = () => driver.ExecuteScript("document.getElementById('" + videoId + "').play()", 0);
            Func<double> getVideoVolume = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').volume", 0).ParseMystery();
            Func<double, bool> videoVolume = expected => Math.Abs(getVideoVolume() - expected) < Epsilon;
            Func<bool> videoMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').muted", 0);
            Func<double> getVideoElapsed = () => driver.ExecuteScript("return document.getElementById('" + videoId + "').currentTime", 0).ParseMystery();
            Func<double, bool> videoElapsed = expected => Math.Abs(getVideoElapsed() - expected) < Epsilon;
            Func<bool> isVideoFullScreen = () => driver.ExecuteScript("return document.webkitFullscreenElement", 0) != null;
            Func<bool> isVideoLoaded = () => (bool)driver.ExecuteScript("return document.getElementById('" + videoId + "').readyState == 4", 0);

            if (!WaitForCondition(isVideoLoaded, attempts: 40))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\nVideo did not load after 20 seconds");
                return;
            }

            // Case 1: tab to play button and play/pause
            TabToElementByName(elements[0], "Play", videoId, driver);
            driver.SendSpecialKey(videoId, WebDriverKey.Space);
            if (!WaitForCondition(videoPlaying))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was not playing after spacebar on play button\n");
                playVideo();
            }

            driver.SendSpecialKey(videoId, WebDriverKey.Enter);
            if (!WaitForCondition(videoPlaying, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was not paused after enter on play button\n");
                pauseVideo();
            }

            // Case 2: Volume and mute
            Javascript.ScrollIntoView(driver, 0);
            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Mute", videoId, driver);
            driver.Screenshot("before_enter_to_mute");

            // mute
            driver.SendSpecialKey(videoId, WebDriverKey.Enter);
            driver.Screenshot("after_enter_to_mute");
            if (!WaitForCondition(videoMuted))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tEnter did not mute the video\n");
            }

            WaitForCondition(() => elements[0].GetAllDescendants().Any(e => e.CurrentName == "Unmute"));

            driver.Screenshot("before_enter_to_unmute");

            // unmute
            driver.SendSpecialKey(videoId, WebDriverKey.Enter);
            driver.Screenshot("after_enter_to_unmute");
            if (!WaitForCondition(videoMuted, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tEnter did not unmute the video\n");
            }

            var initial = getVideoVolume();

            // volume down
            driver.SendSpecialKeys(videoId, new List<WebDriverKey> { WebDriverKey.Arrow_down, WebDriverKey.Arrow_down });
            if (!WaitForCondition(videoVolume, initial - 0.1))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not decrease with arrow keys\n");
            }

            // volume up
            driver.SendSpecialKeys(videoId, new List<WebDriverKey> { WebDriverKey.Arrow_up, WebDriverKey.Arrow_up });
            if (!WaitForCondition(videoVolume, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not increase with arrow keys\n");
            }

            // Case 3: Audio selection
            // TODO test manually

            // Case 4: Progress and seek
            if (videoPlaying())
            {
                // this should not be playing
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was playing when it shouldn't have been\n");
            }

            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Seek", videoId, driver);
            initial = getVideoElapsed();

            // skip ahead
            driver.SendSpecialKey(videoId, WebDriverKey.Arrow_right);
            if (!WaitForCondition(videoElapsed, initial + 10))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip forward with arrow right\n");
            }

            // skip back
            driver.SendSpecialKey(videoId, WebDriverKey.Arrow_left);
            if (!WaitForCondition(videoElapsed, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip back with arrow left\n");
            }

            // Case 5: Progress and seek on remaining time
            if (videoPlaying())
            {
                // this should not be playing
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo was playing when it shouldn't have been\n");
            }

            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Seek", videoId, driver);
            initial = getVideoElapsed();

            // skip ahead
            driver.SendSpecialKey(videoId, WebDriverKey.Arrow_right);
            if (!WaitForCondition(videoElapsed, initial + 10))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip forward with arrow right\n");
            }

            // skip back
            driver.SendSpecialKey(videoId, WebDriverKey.Arrow_left);
            if (!WaitForCondition(videoElapsed, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not skip back with arrow left\n");

                // skip back
                driver.SendSpecialKey(videoId, WebDriverKey.Arrow_left);
            }

            // Case 6: Full screen
            Javascript.ClearFocus(driver, 0);
            TabToElementByName(elements[0], "Full screen", videoId, driver);
            driver.Screenshot("before_enter_to_fullscreen");

            // enter fullscreen mode
            driver.SendSpecialKey(videoId, WebDriverKey.Enter);
            driver.Screenshot("after_enter_to_fullscreen");
            if (!WaitForCondition(isVideoFullScreen))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVideo did not enter FullScreen mode\n");
            }

            driver.Screenshot("before_escape_from_fullscreen");
            driver.SendSpecialKey(videoId, WebDriverKey.Escape);
            driver.Screenshot("after_escape_from_fullscreen");
            if (!WaitForCondition(isVideoFullScreen, reverse: true))
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
        /// <param name="result">The result aggregator</param>
        private static void CheckAudioKeyboardInteractions(List<IUIAutomationElement> elements, DriverManager driver, List<string> ids, TestCaseResultExt result)
        {
            const string audioId = "audio1";
            Func<bool> audioPlaying = () => (bool)driver.ExecuteScript("return !document.getElementById('" + audioId + "').paused", 0);
            Func<object> pauseAudio = () => driver.ExecuteScript("!document.getElementById('" + audioId + "').pause()", 0);
            Func<object> playAudio = () => driver.ExecuteScript("!document.getElementById('" + audioId + "').play()", 0);
            Func<double> getAudioVolume = () => driver.ExecuteScript("return document.getElementById('" + audioId + "').volume", 0).ParseMystery();
            Func<double, bool> audioVolume = expected => Math.Abs(getAudioVolume() - expected) < Epsilon;
            Func<bool> audioMuted = () => (bool)driver.ExecuteScript("return document.getElementById('" + audioId + "').muted", 0);
            Func<double> getAudioElapsed = () => driver.ExecuteScript("return document.getElementById('" + audioId + "').currentTime", 0).ParseMystery();
            Func<double, bool> audioElapsed = expected => Math.Abs(getAudioElapsed() - expected) < Epsilon;
            Func<bool> isAudioLoaded = () => (bool)driver.ExecuteScript("return document.getElementById('" + audioId + "').readyState == 4", 0);

            if (!WaitForCondition(isAudioLoaded, attempts: 40))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\nAudio did not load after 20 seconds");
                return;
            }

            // Case 1: Play/Pause

            // Tab to play button
            driver.SendTabs(audioId, 1);
            driver.SendSpecialKey(audioId, WebDriverKey.Enter);
            if (!WaitForCondition(audioPlaying))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not play with enter\n");
                playAudio();
            }

            driver.SendSpecialKey(audioId, WebDriverKey.Space);
            if (!WaitForCondition(audioPlaying, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not pause with space\n");
                pauseAudio();
            }

            // Case 2: Seek
            if (audioPlaying())
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio was playing when it shouldn't have been\n");
            }

            driver.SendTabs(audioId, 3);
            var initial = getAudioElapsed();
            driver.SendSpecialKey(audioId, WebDriverKey.Arrow_right);
            if (!WaitForCondition(audioElapsed, initial + 10))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not skip forward with arrow right\n");
            }

            driver.SendSpecialKey(audioId, WebDriverKey.Arrow_left);
            if (!WaitForCondition(audioElapsed, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio did not skip back with arrow left\n");
            }

            // Case 3: Volume and mute
            Javascript.ClearFocus(driver, 0);
            driver.SendTabs(audioId, 5);
            initial = getAudioVolume();
            driver.SendSpecialKey(audioId, WebDriverKey.Arrow_down);
            if (!WaitForCondition(audioVolume, initial - .05))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not decrease with arrow down\n");
            }

            driver.SendSpecialKey(audioId, WebDriverKey.Arrow_up);
            if (!WaitForCondition(audioVolume, initial))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tVolume did not increase with arrow up\n");
            }

            driver.SendSpecialKey(audioId, WebDriverKey.Enter);
            if (!WaitForCondition(audioMuted))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio was not muted by enter on the volume control\n");
            }

            driver.SendSpecialKey(audioId, WebDriverKey.Enter);
            if (!WaitForCondition(audioMuted, reverse: true))
            {
                result.Result = ResultType.Half;
                result.AddInfo("\tAudio was not unmuted by enter on the volume control\n");
            }
        }

        /// <summary>
        /// Test all date/time elements except for datetime local, which is tested by the
        /// amended method below.
        /// </summary>
        /// <param name="fields">A count of the number of fields to test</param>
        /// <param name="outputFields">A count of the number of output fields to test</param>
        /// <returns>An action for checking date/time elements</returns>
        private static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckCalendar(int fields, int outputFields = -1)
        {
            return (elements, driver, ids, result) =>
            {
                // set to the number of fields by default
                outputFields = outputFields == -1 ? fields : outputFields;

                var previousControllerForElements = new HashSet<int>();
                foreach (var id in ids.Take(1))
                {
                    // Make sure that the element has focus (gets around weirdness in WebDriver)
                    driver.SendSpecialKeys(
                        id,
                        new List<WebDriverKey>
                        {
                            WebDriverKey.Enter,
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Enter
                        });

                    Func<string> dateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                    Func<string> activeElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                    var today = dateValue();

                    // Open the menu
                    driver.SendSpecialKeys(
                        id,
                        new List<WebDriverKey>
                        {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait
                        });

                    // Check ControllerFor
                    var controllerForElements = elements
                        .Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0)
                        .Select(element => elements.IndexOf(element));
                    if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement controller for not set for id: " + id);
                    }

                    var keys = new List<WebDriverKey>(2)
                    {
                        WebDriverKey.Arrow_down,
                        WebDriverKey.Tab
                    };
                    // Change each field in the calendar
                    for (int i = 0; i < fields; i++)
                    {
                        driver.SendSpecialKeys(id, keys);
                    }

                    // Close the menu (only necessary for time)
                    driver.SendSpecialKey(id, WebDriverKey.Enter);

                    // Get the altered value, which should be one off the default
                    // for each field
                    var newdate = dateValue();
                    var newdatesplit = newdate.Split('-', ':');
                    var todaysplit = today.Split('-', ':');

                    // ensure that all fields have been changed
                    for (int i = 0; i < outputFields; i++)
                    {
                        if (newdatesplit[i] == todaysplit[i])
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nNot all fields were changed by keyboard interaction.");
                        }
                    }

                    var fieldTabs = new List<WebDriverKey>(fields);
                    for (var i = 0; i < fields; i++)
                    {
                        fieldTabs.Add(WebDriverKey.Tab);
                    }

                    keys = new List<WebDriverKey>(2)
                    {
                        WebDriverKey.Enter,
                        WebDriverKey.Wait
                    };

                    keys.AddRange(fieldTabs);
                    driver.SendSpecialKeys(id, keys);

                    var initial = string.Empty;

                    // Check that the accept and cancel buttons are in the tab order
                    if (activeElement() != id)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nUnable to get to accept/dismiss buttons by tab");
                    }

                    // only try to use the buttons if they're there
                    else
                    {
                        initial = dateValue();

                        // **Dismiss button**
                        // Open the dialog, change a field, tab to cancel button, activate it with space,
                        // check that tabbing moves to the previous button (on the page not the dialog)
                        keys = new List<WebDriverKey>
                        {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down
                        };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(
                            new List<WebDriverKey>
                            {
                                WebDriverKey.Tab,
                                WebDriverKey.Space,
                                WebDriverKey.Shift,
                                WebDriverKey.Tab,
                                WebDriverKey.Shift
                            });
                        driver.SendSpecialKeys(id, keys);
                        if (initial != dateValue() || activeElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via space");
                        }

                        // do the same as above, but activate the button with enter this time
                        keys = new List<WebDriverKey>
                        {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down
                        };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(
                            new List<WebDriverKey>
                            {
                                WebDriverKey.Tab,
                                WebDriverKey.Enter,
                                WebDriverKey.Shift,
                                WebDriverKey.Tab,
                                WebDriverKey.Shift
                            });
                        driver.SendSpecialKeys(id, keys);
                        if (initial != dateValue() || activeElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via enter");
                        }

                        // **Accept button**
                        initial = dateValue();

                        // Open the dialog, change a field, tab to accept button, activate it with space,
                        // send tab (since the dialog should be closed, this will transfer focus to the next
                        // input-color button)
                        keys = new List<WebDriverKey>
                        {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down
                        };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(new List<WebDriverKey>
                        {
                            WebDriverKey.Space,
                            WebDriverKey.Tab
                        });
                        driver.SendSpecialKeys(id, keys);
                        if (initial == dateValue() || activeElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via space. Found value: " + dateValue() + " with active element: " + activeElement());
                        }

                        // the value hopefully changed above, but just to be safe
                        initial = dateValue();

                        // Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                        // We don't have to worry about why the dialog closed here (button or global enter)
                        keys = new List<WebDriverKey>
                        {
                            WebDriverKey.Escape,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down
                        };
                        keys.AddRange(fieldTabs);
                        keys.AddRange(
                            new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Tab
                            });
                        driver.SendSpecialKeys(id, keys);
                        if (initial == dateValue() || activeElement() == id)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via enter. Found value: " + dateValue() + " with active element: " + activeElement());
                        }
                    }

                    // open with space, close with enter
                    initial = dateValue();
                    driver.SendSpecialKeys(
                        id,
                        new List<WebDriverKey>
                        {
                            WebDriverKey.Escape,
                            WebDriverKey.Space,
                            WebDriverKey.Arrow_down,
                            WebDriverKey.Enter
                        });
                    if (dateValue() == initial)
                    {
                        result.AddInfo("\nUnable to open dialog with space");
                    }
                }

                foreach (var element in elements)
                {
                    const string patternName = "ValuePattern";
                    var patterned = GetPattern<IUIAutomationValuePattern>(patternName, element);
                    if (patterned == null)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement did not support " + patternName);
                    }
                    else
                    {
                        if (patterned.CurrentValue == null || patterned.CurrentValue == string.Empty)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have value.value set");
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Test the datetime local input element with an amended version of the method
        /// above.
        /// </summary>
        /// <returns>An action to test the datetime local element</returns>
        private static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckDatetimeLocal()
        {
            return (elements, driver, ids, result) =>
            {
                var inputFields = new List<int>
                {
                    3,
                    3
                };

                const int outputFields = 5;

                var previousControllerForElements = new HashSet<int>();
                foreach (var id in ids.Take(1))
                {
                    // Make sure that the element has focus (gets around weirdness in WebDriver)
                    driver.SendSpecialKeys(
                        id,
                        new List<WebDriverKey>
                        {
                            WebDriverKey.Enter,
                            WebDriverKey.Enter,
                            WebDriverKey.Escape
                        });

                    Func<string> DateValue = () => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                    Func<string> ActiveElement = () => (string)driver.ExecuteScript("return document.activeElement.id", 0);

                    driver.SendSpecialKeys(
                        id,
                        new List<WebDriverKey>
                        {
                            WebDriverKey.Tab,
                            WebDriverKey.Enter,
                            WebDriverKey.Enter,
                            WebDriverKey.Tab,
                            WebDriverKey.Enter,
                            WebDriverKey.Enter
                        });
                    var today = DateValue();

                    // Open the date menu
                    driver.SendSpecialKeys(
                        id,
                        new List<WebDriverKey>
                        {
                            WebDriverKey.Shift,
                            WebDriverKey.Tab,
                            WebDriverKey.Enter,
                            WebDriverKey.Wait
                        });

                    // Check ControllerFor
                    var controllerForElements = elements
                        .Where(e => e.CurrentControllerFor != null && e.CurrentControllerFor.Length > 0)
                        .Select(element => elements.IndexOf(element));
                    if (controllerForElements.All(element => previousControllerForElements.Contains(element)))
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement controller for not set for id: " + id);
                    }

                    // NB ++i later
                    for (var i = 0; i < inputFields.Count;)
                    {
                        // Change each field in the calendar
                        for (var j = 0; j < inputFields[i]; j++)
                        {
                            driver.SendSpecialKeys(
                                id,
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Arrow_down,
                                    WebDriverKey.Tab
                                });
                        }

                        driver.SendSpecialKeys(
                            id,
                            new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Tab
                            });

                        // send Enter to open menu
                        if (++i != inputFields.Count)
                        {
                            // expand menu
                            driver.SendSpecialKey(id, WebDriverKey.Enter);
                        }
                    }

                    // Get the altered value, which should be one off the default
                    // for each field
                    var newdate = DateValue();
                    var newdatesplit = newdate.Split('-', ':', 'T');
                    var todaysplit = today.Split('-', ':', 'T');

                    // ensure that all fields have been changed
                    for (int i = 0; i < outputFields; i++)
                    {
                        if (newdatesplit[i] == todaysplit[i])
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nNot all fields were changed by keyboard interaction.");
                        }
                    }

                    var initial = string.Empty;

                    var fieldTabs = new List<WebDriverKey>(inputFields[0]);
                    for (var i = 0; i < inputFields[0]; i++)
                    {
                        fieldTabs.Add(WebDriverKey.Tab);
                    }
                    for (var i = 0; i < inputFields.Count; i++)
                    {
                        var secondPass = i == 1;

                        initial = DateValue();

                        // **Dismiss button**
                        // Open the dialog, change a field, tab to cancel button, activate it with space,
                        // check that tabbing moves to the previous button (on the page not the dialog)
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
                            keys.AddRange(
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Tab,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Space,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Shift,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Shift
                                });
                        }

                        // same as above except without the tab to start
                        else
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Tab,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Space,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Shift,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Shift
                                });
                        }

                        driver.SendSpecialKeys(id, keys);

                        if (initial != DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via space");
                        }

                        // do the same as above, but activate the button with enter this time
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
                            keys.AddRange(
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Tab,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Shift,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Shift
                                });
                        }

                        // same as above except without the tab to start
                        else
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(
                                new List<WebDriverKey>
                                {
                                    WebDriverKey.Tab,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Enter,
                                    WebDriverKey.Wait,
                                    WebDriverKey.Shift,
                                    WebDriverKey.Tab,
                                    WebDriverKey.Shift
                                });
                        }

                        driver.SendSpecialKeys(id, keys);

                        if (initial != DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to cancel with dismiss button via enter");
                        }

                        // **Accept button**
                        initial = DateValue();

                        // Open the dialog, change a field, tab to accept button, activate it with space,
                        // send tab (since the dialog should be closed, this will transfer focus to the next
                        // button)
                        if (secondPass)
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Escape,
                                WebDriverKey.Tab,
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down,
                                WebDriverKey.Wait
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey>
                            {
                                WebDriverKey.Wait,
                                WebDriverKey.Space,
                                WebDriverKey.Wait,
                                WebDriverKey.Tab
                            });
                        }
                        else
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Escape,
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down,
                                WebDriverKey.Wait
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey>
                            {
                                WebDriverKey.Wait,
                                WebDriverKey.Space,
                                WebDriverKey.Wait,
                                WebDriverKey.Tab
                            });
                        }

                        driver.SendSpecialKeys(id, keys);
                        if (initial == DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via space");
                        }

                        // the value hopefully changed above, but just to be safe
                        initial = DateValue();

                        // Open the dialog, tab to hue, change hue, tab to accept button, activate it with enter
                        // We don't have to worry about why the dialog closed here (button or global enter)
                        if (secondPass)
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Escape,
                                WebDriverKey.Tab,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Tab
                            });
                        }
                        else
                        {
                            keys = new List<WebDriverKey>
                            {
                                WebDriverKey.Escape,
                                WebDriverKey.Wait,
                                WebDriverKey.Enter,
                                WebDriverKey.Wait,
                                WebDriverKey.Arrow_down
                            };
                            keys.AddRange(fieldTabs);
                            keys.AddRange(new List<WebDriverKey>
                            {
                                WebDriverKey.Enter,
                                WebDriverKey.Tab
                            });
                        }

                        driver.SendSpecialKeys(id, keys);
                        if (initial == DateValue())
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nUnable to accept with accept button via enter");
                        }
                    }

                    // open with space, close with enter
                    initial = DateValue();
                    driver.SendSpecialKeys(
                        id,
                        new List<WebDriverKey>
                        {
                            WebDriverKey.Escape,
                            WebDriverKey.Space,
                            WebDriverKey.Wait,
                            WebDriverKey.Wait,
                            WebDriverKey.Arrow_down,
                            WebDriverKey.Enter
                        });
                    if (DateValue() == initial)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nUnable to open dialog with space");
                    }
                }

                foreach (var element in elements)
                {
                    const string patternName = "ValuePattern";
                    var patterned = GetPattern<IUIAutomationValuePattern>(patternName, element);
                    if (patterned == null)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nElement did not support " + patternName);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(patterned.CurrentValue))
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement did not have value.value set");
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Func factory for checking that when invalid input is entered into a form,
        /// an error message appears.
        /// </summary>
        /// <returns>An action for checking input validation</returns>
        private static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckValidation()
        {
            return (elements, driver, ids, result) =>
                {
                    // The indices of the elements that have been found to be invalid before
                    foreach (var id in ids.Take(1))
                    {
                        Javascript.ScrollIntoView(driver, 0);

                        driver.SendKeys(id, "invalid");
                        driver.SendSpecialKey(id, WebDriverKey.Enter);

                        // Everything that is invalid on the page
                        // We search by both with an OR condition because it gives a better chance to
                        // find elements that are partially correct.
                        var invalid = elements.Where(e => e.CurrentIsDataValidForForm == 0).ToList();

                        if (invalid.Count > 1)
                        {
                            throw new Exception("Test assumption failed, multiple elements were invalid");
                        }

                        if (invalid.Count < 1)
                        {
                            result.Result = ResultType.Half;
                            result.AddInfo("\nElement failed to validate improper input");
                        }

                        var invalidElement = invalid.First();

                        if (!WaitForCondition(() => invalidElement.CurrentControllerFor.Length == 1, () => driver.SendSpecialKey(id, WebDriverKey.Enter)))
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

                        if (string.IsNullOrEmpty(invalidElement.CurrentHelpText))
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
        /// <param name="searchStrategy">A strategy to be used when looking for elements</param>
        /// <returns>A Func that can be used to verify whether the elements in the list are child elements</returns>
        private static Action<List<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckChildNames(
            List<string> requiredNames,
            Func<IUIAutomationElement, bool> searchStrategy = null)
        {
            return (elements, driver, ids, result) =>
            {
                foreach (var element in elements)
                {
                    var names = element.GetChildNames(searchStrategy);

                    // get a list of all required names not found
                    var expectedNotFound = requiredNames.Where(rn => !names.Contains(rn)).ToList();

                    // get a list of all found names that weren't required
                    var foundNotExpected = names.Where(n => !requiredNames.Contains(n)).ToList();

                    if (expectedNotFound.Count > 0)
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo(
                            expectedNotFound.Any() ? "\n" +
                                expectedNotFound.Aggregate((a, b) => a + ", " + b) +
                                (expectedNotFound.Count > 1 ?
                                    " were expected as names but not found. " :
                                    " was expected as a name but not found. ")
                                : string.Empty);
                        result.AddInfo(
                            foundNotExpected.Any() ? "\n" +
                                foundNotExpected.Aggregate((a, b) => a + ", " + b) +
                                (foundNotExpected.Count > 1 ?
                                    " were found as names but not expected. " :
                                    " was found as a name but not expected. ")
                                : string.Empty);
                    }
                }
            };
        }

        /// <summary>
        /// Check that the clear button can be tabbed to and that it can be activated with space
        /// </summary>
        /// <returns>An action that checks the clear button</returns>
        private static Action<IEnumerable<IUIAutomationElement>, DriverManager, List<string>, TestCaseResultExt> CheckClearButton()
        {
            return (elements, driver, ids, result) =>
            {
                Func<string, string> inputValue = (id) => (string)driver.ExecuteScript("return document.getElementById('" + id + "').value", 0);
                Action<string> clearInput = (id) => driver.ExecuteScript("document.getElementById('" + id + "').value = ''", 0);

                foreach (var id in ids.Take(1))
                {
                    // Enter something, tab to the clear button, clear with space
                    driver.SendKeys(id, "x");
                    driver.SendSpecialKey(id, WebDriverKey.Wait);
                    if (!elements.Any(e => e.GetAllDescendants().Any(d => d.CurrentName.ToLowerInvariant().Contains("clear value"))))
                    {
                        result.Result = ResultType.Half;
                        result.AddInfo("\nUnable to find a clear button as a child of any element");
                    }

                    // Don't leave input which could cause problems with other tests
                    clearInput(id);
                }
            };
        }

        /// <summary>
        /// Wait for a condition using a given checker
        /// </summary>
        /// <param name="conditionCheck">A function that will tell when the function has waited long enough</param>
        /// <param name="value">The double value to be passed into the condition check func</param>
        /// <param name="attempts">How many times to wait</param>
        /// <returns>Whether the condition was ever successful</returns>
        private static bool WaitForCondition(Func<double, bool> conditionCheck, double value, int attempts = 20)
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
        /// Wait for a condition using a given checker
        /// </summary>
        /// <param name="conditionCheck">A function that will tell when the function has waited long enough</param>
        /// <param name="waitAction">An optional action to take between waits</param>
        /// <param name="reverse">If the function should wait until the condition check should return false instead of true</param>
        /// <param name="attempts">How many times to wait</param>
        /// <returns>Whether the condition was ever successful</returns>
        private static bool WaitForCondition(Func<bool> conditionCheck, Action waitAction = null, bool reverse = false, int attempts = 20)
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
