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
    internal abstract class TestStrategy
    {
        protected DriverManager _driverManager;
        protected string _RepositoryPath;
        protected string _FileSuffix;

        private string BuildTestUrl(string testName)
        {
            return _RepositoryPath + testName;
        }

        /// <summary>
        /// This method is pretty minimal, most of the work is done in the 
        /// DefaultTestCase method below.
        /// </summary>
        /// <param name="testData">An object that describes the test</param>
        /// <returns></returns>
        public IEnumerable<TestCaseResult> Execute(TestData testData)
        {
            _driverManager.NavigateToUrl(BuildTestUrl(testData.Name + _FileSuffix));
            return TestElement(testData);
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
        internal abstract IEnumerable<TestCaseResult> TestElement(TestData testData);

        /// <summary>
        /// This wrapper is used to report 100% for a test
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected List<TestCaseResult> Pass(string name)
        {
            return new List<TestCaseResult> {
                        new TestCaseResult{
                            Result = ResultType.Pass,
                            Name = name + "-1",
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
        protected List<TestCaseResult> Fail(string name, string cause)
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
        protected List<TestCaseResult> Half(string name, string cause)
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
        protected IEnumerable<TestCaseResult> Skip(string testName)
        {
            return Enumerable.Empty<TestCaseResult>();
        }

        internal void Close()
        {
            _driverManager.Close();
        }
    }
}
