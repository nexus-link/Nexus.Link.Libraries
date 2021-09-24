using System.Collections.Generic;
using System.Net;
using Microsoft.Extensions.Primitives;

namespace Nexus.Link.Libraries.Web.Serialization
{
    public class ResponseData
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
        public Dictionary<string, StringValues> Headers { get; set; } = new Dictionary<string, StringValues>();
        public string BodyAsString { get; set; }
    }
}
