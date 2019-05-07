using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Nexus.Link.Libraries.Web.Logging.Data
{
    public class HttpRequestData
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Route { get; set; }
        public Dictionary<string, string[]> Headers { get; set; }
        public string Body { get; set; }
    }
}
