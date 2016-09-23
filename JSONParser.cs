using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using static Microsoft.Edge.A11y.ElementConverter;
using System.Linq;
using System.IO;

namespace Microsoft.Edge.A11y
{
    static class JSONParser
    {
        public static IEnumerable<TestData> SampleJsonTest()
        {
            try
            {
                var sample = File.ReadAllText("sample.json");
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

        private static TestData ParseElement(dynamic element, string name, bool recursive = false)
        {
            UIAControlType controlType = UIAControlType.Unknown;
            try
            {
                controlType = Enum.Parse(typeof(UIAControlType), element.ControlType.ToString());
            }
            catch { }

            if (recursive)
            {
                try
                {
                    name = element.name;
                }
                catch { }
            }

            if(controlType == UIAControlType.Unknown && !recursive)
            {
                throw new Exception("Element did not have control type");
            }

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
                    children.Add(ParseElement(child, name, true));
                }
            }
            catch { }

            return new TestData(name, controlType, localizedControlType, requiredPatterns: patterns, children: children);
        }
    }
}