using Interop.UIAutomationCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Edge.A11y
{
    /// <summary>
    /// A strategy for testing Edge Accessibility as scored at 
    /// http://html5accessibility.com/
    /// </summary>
    public class EdgeStrategy
    {
        string _RepositoryPath;

        public EdgeStrategy(DriverManager driverManager, string repositoryPath = "https://cdn.rawgit.com/DHBrett/AT-browser-tests/gh-pages/test-files/")
        {
            _RepositoryPath = repositoryPath;
        }

        /// <summary>
        /// This method is pretty minimal, most of the work is done in the 
        /// DefaultTestCase method below.
        /// </summary>
        /// <param name="driverManager"></param>
        /// <param name="type"></param>
        /// <param name="suite"></param>
        /// <param name="version"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public IEnumerable<TestCaseResult> Execute(DriverManager driverManager, TestData testData)
        {
            driverManager.NavigateToUrl(BuildTestUrl(testData._TestName + ".html"));
            return testData._ControlType == null ? Skip(testData._TestName) : TestElement(testData, driverManager);
        }

        private string BuildTestUrl(string testName)
        {
            return _RepositoryPath + testName;
        }

        /// <summary>
        /// This handles most of the work of the test cases.
        ///
        /// N.B. all the test case results are returned in pairs, since we need to be
        /// able to give half scores for certain results.
        /// </summary>
        /// <param name="testData">An object which stores information about the
        /// expected results</param>
        /// <param name="driverManager"></param>
        /// <returns></returns>
        private IEnumerable<TestCaseResult> TestElement(TestData testData, DriverManager driverManager)
        {
            //Find the browser
            var browserElement = EdgeA11yTools.FindBrowserDocument(0);
            if (browserElement == null)
            {
                return Fail(testData._TestName, "Unable to find the browser");
            }

            //Find elements using ControlType or the alternate search strategy
            var testElements = EdgeA11yTools.SearchDocumentChildren(browserElement, testData._ControlType, testData._SearchStrategy);
            if (testElements.Count == 0)
            {
                return Fail(testData._TestName, "Unable to find the element");
            }
            
            //This is used if the test passes but there is something to report
            string note = null;
            var elementConverter = new ElementConverter();

            //If necessary, check localized control type
            if (testData._LocalizedControlType != null)
            {
                foreach (var element in testElements)
                {
                    if (element.CurrentLocalizedControlType.Equals(testData._LocalizedControlType, StringComparison.OrdinalIgnoreCase))
                    {
                        return Half(testData._TestName, "Element did not have the correct localized control type");
                    }


                }
            }

            //If necessary, check landmark and localized landmark types
            if (testData._LandmarkType != null)
            {
                foreach (var element in testElements)
                {
                    var five = element as IUIAutomationElement5;
                    var convertedLandmark = elementConverter.GetElementNameFromCode(five.CurrentLandmarkType);
                    var localizedLandmark = five.CurrentLocalizedLandmarkType;

                    if (convertedLandmark != testData._LandmarkType)
                    {
                        return Half(testData._TestName, "Element did not have the correct landmark type\n");
                    }

                    if (localizedLandmark != testData._LocalizedLandmarkType)
                    {
                        return Half(testData._TestName, "Element did not have the correct localized landmark type\n");
                    }
                }
            }

            //If necessary, check keboard accessibility
            var tabbable = EdgeA11yTools.TabbableIds(driverManager);
            if (testData._KeyboardElements != null && testData._KeyboardElements.Count > 0)
            {
                foreach (var e in testData._KeyboardElements)
                {
                    if (!tabbable.Contains(e))
                    {
                        return Half(testData._TestName, "Could not access element with id: '" + e + "' by tab");
                    }
                }
            }


            try
            {
                //If necessary, check any additional requirements
                if (testData._AdditionalRequirement != null)
                {
                    if (!testData._AdditionalRequirement(testElements, driverManager, tabbable))
                    {
                        return Half(testData._TestName, "Failed additional requirement");
                    }
                }
            }
            catch(Exception ex)
            {
                return Half(testData._TestName, "Caught exception during test execution, ERROR: " + ex.Message + "\nCallStack:\n" + ex.StackTrace);
            }

            return Pass(testData._TestName, note);
        }

        /// <summary>
        /// This wrapper is used to report 100% for a test
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private List<TestCaseResult> Pass(string name, string note)
        {
            return new List<TestCaseResult> {
                        new TestCaseResult{
                            Result = ResultType.Pass,
                            Name = name + "-1",
                            MoreInfo = note
                        }, 
                        new TestCaseResult{
                            Result = ResultType.Pass,
                            Name = name + "-2"
                        }
                    };
        }

        /// <summary>
        /// This wrapper is used to report 0% for a test
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private List<TestCaseResult> Fail(string name, string cause)
        {
            return new List<TestCaseResult> {
                        new TestCaseResult{
                            Result = ResultType.Fail,
                            Name = name + "-1",
                            MoreInfo = cause
                        }, 
                        new TestCaseResult{
                            Result = ResultType.Fail,
                            Name = name + "-2",
                            MoreInfo = cause
                        }
                    };
        }

        /// <summary>
        /// This wrapper is used to report 50% for a test
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private List<TestCaseResult> Half(string name, string cause)
        {
            return new List<TestCaseResult> {
                        new TestCaseResult{
                            Result = ResultType.Pass,
                            Name = name + "-1"
                        }, 
                        new TestCaseResult{
                            Result = ResultType.Fail,
                            Name = name + "-2",
                            MoreInfo = cause
                        }
                    };
        }

        /// <summary>
        /// This is used to skip a test without influencing the overall score
        /// </summary>
        /// <param name="testName"></param>
        /// <returns></returns>
        private IEnumerable<TestCaseResult> Skip(string testName)
        {
            return Enumerable.Empty<TestCaseResult>();
        }
    }
}
