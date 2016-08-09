namespace Microsoft.Edge.A11y
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A strategy for testing Edge Accessibility as scored at 
    /// http://html5accessibility.com/
    /// </summary>
    internal abstract class TestStrategy
    {
        /// <summary>
        /// Gets or sets the WebDriver handle
        /// </summary>
        protected DriverManager DriverManager { get; set; }

        /// <summary>
        /// Gets or sets the path to the base repository
        /// </summary>
        protected string RepositoryPath { get; set; }

        /// <summary>
        /// Gets or sets a suffix to add when creating URLs
        /// </summary>
        protected string FileSuffix { get; set; }

        /// <summary>
        /// This method is pretty minimal, most of the work is done in the 
        /// DefaultTestCase method below.
        /// </summary>
        /// <param name="testData">An object that describes the test</param>
        /// <returns>The results of the test</returns>
        public IEnumerable<TestCaseResult> Execute(TestData testData)
        {
            this.DriverManager.NavigateToUrl(this.BuildTestUrl(testData.TestName + this.FileSuffix));
            return this.TestElement(testData);
        }

        /// <summary>
        /// Closes the WebDriver session
        /// </summary>
        internal void Close()
        {
            this.DriverManager.Close();
        }

        /// <summary>
        /// This handles most of the work of the test cases.
        /// </summary>
        /// <param name="testData">An object which stores information about the
        /// expected results</param>
        /// <returns>The results of the test</returns>
        internal abstract IEnumerable<TestCaseResult> TestElement(TestData testData);

        /// <summary>
        /// This wrapper is used to report 100% for a test
        /// </summary>
        /// <param name="name">The name of the test</param>
        /// <returns>Two TestCaseResults which are both marked as failures</returns>
        protected List<TestCaseResult> Pass(string name)
        {
            return new List<TestCaseResult>
            {
                new TestCaseResult
                {
                    Result = ResultType.Pass,
                    Name = name + "-1",
                },
                new TestCaseResult
                {
                    Result = ResultType.Pass,
                    Name = name + "-2"
                }
            };
        }

        /// <summary>
        /// This wrapper is used to report 0% for a test
        /// </summary>
        /// <param name="name">The name of the test</param>
        /// <param name="cause">The cause for failure</param>
        /// <returns>Two TestCaseResults which are both marked as failures</returns>
        protected List<TestCaseResult> Fail(string name, string cause)
        {
            return new List<TestCaseResult>
            {
                new TestCaseResult
                {
                    Result = ResultType.Fail,
                    Name = name + "-1",
                    MoreInfo = cause
                },
                new TestCaseResult
                {
                    Result = ResultType.Fail,
                    Name = name + "-2",
                    MoreInfo = cause
                }
            };
        }

        /// <summary>
        /// This wrapper is used to report 50% for a test
        /// </summary>
        /// <param name="name">The name of the test</param>
        /// <param name="cause">The cause for failure</param>
        /// <returns>Two TestCaseResults, one pass and one fail</returns>
        protected List<TestCaseResult> Half(string name, string cause)
        {
            return new List<TestCaseResult>
            {
                new TestCaseResult
                {
                    Result = ResultType.Pass,
                    Name = name + "-1"
                },
                new TestCaseResult
                {
                    Result = ResultType.Fail,
                    Name = name + "-2",
                    MoreInfo = cause
                }
            };
        }

        /// <summary>
        /// This is used to skip a test without influencing the overall score
        /// </summary>
        /// <param name="testName">The name of the test</param>
        /// <returns>An empty list of test</returns>
        protected IEnumerable<TestCaseResult> Skip(string testName)
        {
            return Enumerable.Empty<TestCaseResult>();
        }

        /// <summary>
        /// Create a complete URL from a test name
        /// </summary>
        /// <param name="testName">The test name</param>
        /// <returns>A complete url for a given test</returns>
        private string BuildTestUrl(string testName)
        {
            return this.RepositoryPath + testName;
        }
    }
}
