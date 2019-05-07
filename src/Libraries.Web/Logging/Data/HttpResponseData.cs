using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Nexus.Link.Libraries.Web.Logging.Data
{
    public class HttpResponseData
    {
        public Dictionary<string, string[]> Headers { get; set; }
        public string Body { get; set; }
        public int StatusCode { get; set; }
        public double? ElapsedSeconds { get; set; }
    }
}
