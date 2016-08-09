namespace Microsoft.Edge.A11y
{
    using System.Text;

    public enum ResultType
    {
        Fail = 0,
        Pass = 1,
        Unknown = 2,
        Half = 3
    }

    public class TestCaseResult
    {
        public ResultType Result { get; set; }
        public string Name { get; set; }
        public string MoreInfo { get; set; }
        public string Browser { get; set; }

        public override string ToString()
        {
            return Name + ":\n\t" + Result + (MoreInfo != null ? " (" + MoreInfo + ")" : "");
        }

        public string ToCSVString()
        {
            return Name + "," + Result + (MoreInfo != null ? "," + MoreInfo : "");
        }
    }


    public class TestCaseResultExt : TestCaseResult
    {
        public TestCaseResultExt()
        {
            Result = ResultType.Pass;
        }

        public void AddInfo(string info)
        {
            var stringBuilder = new StringBuilder(MoreInfo);
            stringBuilder.Append(info);
            MoreInfo = stringBuilder.ToString();
        }
    }
}
