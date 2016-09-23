using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Edge.A11y
{
    class JSONResponse
    {
        public string status {get; set;}
        public string statusText { get; set; }
        public Dictionary<string, string> data;

        public JSONResponse()
        {
            data = new Dictionary<string, string>();
        }
    }
}
