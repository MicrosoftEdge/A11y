using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Microsoft.Edge.A11y.ElementConverter;

namespace Microsoft.Edge.A11y
{
    static class JSONParser
    {
        static string sample = "{ \"id\": \"checkbox\", \"UIA\": { \"elements\": [{ \"ControlType\": \"Checkbox\", \"LocalizedControlType\": \"check box\", \"name\": \"Placeholder content\", \"patterns\": [{ \"name\": \"Toggle\", \"properties\": [{ \"name\": \"ToggleState\", \"value\": \"Off\" }] }] }] } }";

        public static IEnumerable<TestData> SampleJsonTest()
        {
            try
            {
                dynamic converted = JsonConvert.DeserializeObject(sample);
                var UIA = converted.UIA;
                string name, localizedControlType;
                UIAControlType controlType;
                var toreturn = new List<TestData>();
                foreach (var element in UIA.elements)
                {
                    name = converted.id;
                    controlType = Enum.Parse(typeof(UIAControlType), element.ControlType.ToString());

                    localizedControlType = element.LocalizedControlType;

                    var patterns = new List<UIAPattern>();
                    foreach(var pattern in element.patterns)
                    {
                        patterns.Add(Enum.Parse(typeof(UIAPattern), pattern.name.ToString() + "Pattern"));
                    }

                    toreturn.Add(new TestData(name, controlType, localizedControlType, requiredPatterns: patterns));
                }
                return toreturn;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}