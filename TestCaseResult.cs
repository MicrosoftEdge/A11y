using System.Text;

namespace Microsoft.Edge.A11y
{
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
