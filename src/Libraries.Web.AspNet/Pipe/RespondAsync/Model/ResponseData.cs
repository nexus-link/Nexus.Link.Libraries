#if NETCOREAPP
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Nexus.Link.Libraries.Web.AspNet.Pipe.RespondAsync.Model
{
    public class ResponseData
    {

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;
        public HeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public string? BodyAsString { get; set; }
        public async Task<ResponseData> FromAsync(HttpResponseMessage response, bool loadIntoBuffer = false)
        {
            StatusCode = response.StatusCode;
            foreach (var (key, value) in response.Headers)
            {
                Headers.Add(key, value.ToArray());
            }

            if (loadIntoBuffer) await response.Content.LoadIntoBufferAsync();
            BodyAsString = response.Content == null ? null : await response.Content.ReadAsStringAsync();
            return this;
        }

        public ResponseData From(Exception exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            BodyAsString = JsonConvert.SerializeObject($"{exception}");
            return this;
        }
    }
}
#endif