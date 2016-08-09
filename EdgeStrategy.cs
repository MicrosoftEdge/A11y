namespace Microsoft.Edge.A11y
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using static ElementConverter;
    using Interop.UIAutomationCore;

    /// <summary>
    /// A strategy for testing Edge Accessibility as scored at 
    /// http://html5accessibility.com/
    /// </summary>
    internal class EdgeStrategy : TestStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EdgeStrategy"/> class
        /// </summary>
        /// <param name="repositoryPath">The base path for all the elements</param>
        /// <param name="fileSuffix">A suffix to add to each element name when creating the path</param>
        public EdgeStrategy(string repositoryPath = "https://cdn.rawgit.com/DHBrett/AT-browser-tests/gh-pages/test-files/", string fileSuffix = "")
        {
            this.DriverManager = new DriverManager(TimeSpan.FromSeconds(10));
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2)); // Wait for the browser to load before we start searching
            this.RepositoryPath = repositoryPath;
            this.FileSuffix = fileSuffix;
        }

        /// <summary>
        /// This handles most of the work of the test cases.
        /// </summary>
        /// <param name="testData">An object which stores information about the
        /// expected results</param>
        /// <returns>A list of results for a given element</returns>
        internal override IEnumerable<TestCaseResult> TestElement(TestData testData)
        {
            // Find the browser
            var browserElement = EdgeA11yTools.FindBrowserDocument(0);
            if (browserElement == null)
            {
                return this.Fail(testData.TestName, "Unable to find the browser");
            }

            // Find elements using ControlType or the alternate search strategy
            HashSet<UIAControlType> foundControlTypes;
            var testElements = EdgeA11yTools.SearchChildren(browserElement, testData.ControlType, testData.SearchStrategy, out foundControlTypes);
            if (testElements.Count == 0)
            {
                if (testData.SearchStrategy == null)
                {
                    return this.Fail(testData.TestName, "Unable to find the element, found these instead: " + foundControlTypes.Select(ct => ct.ToString()).Aggregate((a, b) => a + ", " + b));
                }

                return this.Fail(testData.TestName, "Unable to find the element using the alternate search strategy");
            }

            var moreInfo = new StringBuilder();

            // If necessary, check localized control type
            if (testData.LocalizedControlType != null)
            {
                foreach (var element in testElements)
                {
                    if (!element.CurrentLocalizedControlType.Equals(testData.LocalizedControlType, StringComparison.OrdinalIgnoreCase))
                    {
                        var error = "\nElement did not have the correct localized control type. Expected:" +
                            testData.LocalizedControlType + " Actual:" + element.CurrentLocalizedControlType;
                        moreInfo.Append(error);
                    }
                }
            }

            // If necessary, check landmark and localized landmark types
            if (testData.LandmarkType != UIALandmarkType.Unknown)
            {
                foreach (var element in testElements)
                {
                    var five = element as IUIAutomationElement5;
                    var convertedLandmark = GetLandmarkTypeFromCode(five.CurrentLandmarkType);
                    var localizedLandmark = five.CurrentLocalizedLandmarkType;

                    if (convertedLandmark != testData.LandmarkType)
                    {
                        var error = "\nElement did not have the correct landmark type. Expected:" +
                            testData.LandmarkType + " Actual:" + convertedLandmark + "\n";
                        moreInfo.Append(error);
                    }

                    if (localizedLandmark != testData.LocalizedLandmarkType)
                    {
                        var error = "\nElement did not have the correct localized landmark type. Expected:" +
                            testData.LocalizedLandmarkType + " Actual:" + localizedLandmark + "\n";
                        moreInfo.Append(error);
                    }
                }
            }

            // If necessary, naming and descriptions
            // This is done "out of order" since the keyboard checks below invalidate the tree
            if (testData.RequiredNames != null || testData.RequiredDescriptions != null)
            {
                CheckElementNames(
                    moreInfo,
                    testElements,
                    testData.RequiredNames ?? new List<string>(),
                    testData.RequiredDescriptions ?? new List<string>());
            }

            // If necessary, check keboard accessibility
            var tabbable = EdgeA11yTools.TabbableIds(DriverManager);
            if (testData.KeyboardElements != null && testData.KeyboardElements.Count > 0)
            {
                foreach (var e in testData.KeyboardElements)
                {
                    if (!tabbable.Contains(e))
                    {
                        moreInfo.Append("\nCould not access element with id: '" + e + "' by tab");
                    }
                }
            }

            try
            {
                // If necessary, check any additional requirements
                if (testData.AdditionalRequirement != null)
                {
                    testElements = EdgeA11yTools.SearchChildren(browserElement, testData.ControlType, testData.SearchStrategy, out foundControlTypes);
                    var additionalRequirementResult = testData.AdditionalRequirement(testElements, DriverManager, tabbable);
                    if (additionalRequirementResult.Result != ResultType.Pass)
                    {
                        moreInfo.AppendLine(additionalRequirementResult.MoreInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                moreInfo.Append("\nCaught exception during test execution, ERROR: " + ex.Message + "\nCallStack:\n" + ex.StackTrace);
            }

            var moreInfoString = moreInfo.ToString();
            if (moreInfoString != string.Empty)
            {
                return this.Half(testData.TestName, moreInfoString.Trim());
            }

            return this.Pass(testData.TestName);
        }

        /// <summary>
        /// Check all the elements for correct naming and descriptions
        /// </summary>
        /// <param name="moreInfo">The StringBuilder which holds more info for the test</param>
        /// <param name="elements">The UIA elements to check</param>
        /// <param name="requiredNames">The names to search for</param>
        /// <param name="requiredDescriptions">The descriptions to search for</param>
        private static void CheckElementNames(StringBuilder moreInfo, List<IUIAutomationElement> elements, List<string> requiredNames, List<string> requiredDescriptions)
        {
            var names = elements.ConvertAll(element => element.CurrentName).Where(e => !string.IsNullOrEmpty(e)).ToList();
            var descriptions = elements.ConvertAll(element => ((IUIAutomationElement6)element).CurrentFullDescription).Where(e => !string.IsNullOrEmpty(e)).ToList();

            // Check names - get a list of all required names not found
            var expectedNotFound = requiredNames.Where(rn => !names.Contains(rn)).ToList();

            // get a list of all found names that weren't required
            var foundNotExpected = names.Where(n => !requiredNames.Contains(n)).ToList();

            moreInfo.Append(
                expectedNotFound.Any() ? "\n" +
                    expectedNotFound.Aggregate((a, b) => a + ", " + b) +
                    (expectedNotFound.Count() > 1 ?
                        " were expected as names but not found. " :
                        " was expected as a name but not found. ")
                    : string.Empty);
            moreInfo.Append(
                foundNotExpected.Any() ? "\n" +
                    foundNotExpected.Aggregate((a, b) => a + ", " + b) +
                    (foundNotExpected.Count() > 1 ?
                        " were found as names but not expected. " :
                        " was found as a name but not expected. ")
                    : string.Empty);

            // Check descriptions
            expectedNotFound = requiredDescriptions.Where(rd => !descriptions.Contains(rd)).ToList();
            foundNotExpected = descriptions.Where(d => !requiredDescriptions.Contains(d)).ToList();
            moreInfo.Append(
                expectedNotFound.Any() ? "\n" +
                    expectedNotFound.Aggregate((a, b) => a + ", " + b) +
                    (expectedNotFound.Count() > 1 ?
                        " were expected as descriptions but not found. " :
                        " was expected as a description but not found. ")
                    : string.Empty);
            moreInfo.Append(
                foundNotExpected.Any() ? "\n" +
                    foundNotExpected.Aggregate((a, b) => a + ", " + b) +
                    (foundNotExpected.Count() > 1 ?
                        " were found as descriptions but not expected. " :
                        " was found as a description but not expected. ")
                    : string.Empty);
        }
    }
}
