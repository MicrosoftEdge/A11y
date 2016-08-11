namespace Microsoft.Edge.A11y
{
    using System.Text;

    /// <summary>
    /// A modified version of TestCaseResult that is used for sub-tests
    /// </summary>
    public class TestCaseResultExt : TestCaseResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseResultExt" /> class
        /// </summary>
        public TestCaseResultExt()
        {
            this.Result = ResultType.Pass;
        }

        /// <summary>
        /// Adds information about the ongoing set of tests
        /// </summary>
        /// <param name="info">A string describing the state of the tests</param>
        public void AddInfo(string info)
        {
            var stringBuilder = new StringBuilder(MoreInfo, MoreInfo.Length + info.Length);
            stringBuilder.Append(info);
            this.MoreInfo = stringBuilder.ToString();
        }
    }
}
