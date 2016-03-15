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
            return Name + ":\n\t" + Result + (MoreInfo != null ? " (" + Truncate(MoreInfo, 30) + ")" : "");
        }

        public string ToCSVString()
        {
            return Name + "," + Result + (MoreInfo != null ? "," + Truncate(MoreInfo, 30) : "");
        }
        
        public static string Truncate(string value, int length)
        {
            return value!= null && value.Length > length ? value.Substring(0, length) : value;
        }
    }
}
