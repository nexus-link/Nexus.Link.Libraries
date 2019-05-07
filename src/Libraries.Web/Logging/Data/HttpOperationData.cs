using System;
using System.Collections.Generic;
using System.Text;

namespace Nexus.Link.Libraries.Web.Logging.Data
{
    public class HttpOperationData
    {
        public HttpRequestData Request { get; set; }
        public HttpResponseData Response { get; set; }
    }
}
