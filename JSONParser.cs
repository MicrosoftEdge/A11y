using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using static Microsoft.Edge.A11y.ElementConverter;
using System.Linq;

namespace Microsoft.Edge.A11y
{
    static class JSONParser
    {
        static string sample = "{ \"id\": \"checkbox\", \"UIA\": { \"elements\": [{ \"ControlType\": \"Checkbox\", \"LocalizedControlType\": \"check box\", \"name\": \"Placeholder content\", \"patterns\": [{ \"name\": \"TogglePattern\", \"properties\": [{ \"name\": \"ToggleState\", \"value\": \"Off\" }] }] }] } }";

        public static IEnumerable<TestData> SampleJsonTest()
        {
            try
            {
                dynamic converted = JsonConvert.DeserializeObject(sample);
                var UIA = converted.UIA;
                var toreturn = new List<TestData>();
                foreach (var element in UIA.elements)
                {
                    toreturn.Add(ParseElement(element, (string)converted.id));
                }
                return toreturn;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private static TestData ParseElement(dynamic element, string name)
        {
            var controlType = Enum.Parse(typeof(UIAControlType), element.ControlType.ToString());

            string localizedControlType = null;
            try
            {
                localizedControlType = element.LocalizedControlType;
            }
            catch { }

            List<UIAPattern> patterns = null;
            try
            {
                patterns = new List<UIAPattern>();
                foreach (var pattern in element.patterns)
                {
                    patterns.Add(Enum.Parse(typeof(UIAPattern), pattern.name.ToString()));
                }
            }
            catch { }

            List<TestData> children = null;
            try
            {
                children = new List<TestData>();
                foreach (var child in element.children)
                {
                    children.Add(ParseElement(child, name));
                }
            }
            catch { }

            return new TestData(name, controlType, localizedControlType, requiredPatterns: patterns, children: children);
        }
    }
}