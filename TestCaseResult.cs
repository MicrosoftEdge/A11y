namespace Microsoft.Edge.A11y
{
    using System.Text;

    /// <summary>
    /// The different types of results that can be returned from a test
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// A complete failure of the test
        /// </summary>
        Fail = 0,

        /// <summary>
        /// A complete pass of the test
        /// </summary>
        Pass = 1,

        /// <summary>
        /// An unknown result
        /// </summary>
        Unknown = 2,

        /// <summary>
        /// A partial failure of the test
        /// </summary>
        Half = 3
    }

    /// <summary>
    /// The container for results returned from tests
    /// </summary>
    public class TestCaseResult
    {
        /// <summary>
        /// Gets or sets the result enum
        /// </summary>
        public ResultType Result { get; set; }

        /// <summary>
        /// Gets or sets the name of the element being tested
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an explanation of the result
        /// </summary>
        public string MoreInfo { get; set; }

        /// <summary>
        /// Gets or sets which browser executed the tests
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// Gets or sets summarizes the result into a form that can be printed to the console
        /// </summary>
        /// <returns>A space-separated set of values describing the result</returns>
        public override string ToString()
        {
            return this.Name + ":\n\t" + this.Result + (this.MoreInfo != null ? " (" + this.MoreInfo + ")" : string.Empty);
        }

        /// <summary>
        /// Summarizes the result into a form that can be added to a csv file
        /// </summary>
        /// <returns>A comma-separated set of values describing the result</returns>
        public string ToCSVString()
        {
            return this.Name + "," + this.Result + (this.MoreInfo != null ? "," + this.MoreInfo : string.Empty);
        }
    }


    public class TestCaseResultExt : TestCaseResult
    {
        StringBuilder _MoreInfo = new StringBuilder();

        new public string MoreInfo
        {
            get
            {
                return _MoreInfo.ToString();
            }
        }

        public TestCaseResultExt()
        {
            Result = ResultType.Pass;
        }

        public void AddInfo(string info)
        {
            _MoreInfo.Append(info);
        }
    }
}
