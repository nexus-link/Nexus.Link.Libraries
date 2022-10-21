using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Model;
using System;
using System.Net;
using JetBrains.Annotations;
using System.Net.Http;
using Nexus.Link.Libraries.Core.Assert;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The request resulted in an HTTP status code in the 3xx range. We will return the Uri for the request and the location header.
    /// </summary>
    public class FulcrumHttpRedirectException : FulcrumRedirectException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public new static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumHttpRedirectException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>
        /// </summary>
        public new const string ExceptionType = "Xlent.Fulcrum.Http.Redirect";

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumHttpRedirectException() : this(null, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumHttpRedirectException(string message) : this(message, (Exception)null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumHttpRedirectException(HttpResponseMessage response) : this(
            $"Redirect from {response?.RequestMessage?.RequestUri} to {response?.Headers?.Location}.", (Exception) null)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            RequestUri = response.RequestMessage?.RequestUri;
            LocationUri = response.Headers?.Location;
            HasRedirectIds = TryInterpretRedirectException(response);
            HttpStatusCode = (int) response.StatusCode;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FulcrumHttpRedirectException(string message, Exception innerException) : base(message, innerException)
        {
            SetProperties(innerException);
            if (innerException is FulcrumException innerFulcrumException) CopyFrom(innerFulcrumException);
        }

        /// <inheritdoc />
        public override bool IsRetryMeaningful { get; set; } = false;

        /// <inheritdoc />
        public override string Type => ExceptionType;

        /// <summary>
        /// The request URI that should be replaced with <see cref="LocationUri"/>.
        /// </summary>
        public bool HasRedirectIds
        {
            get => GetData<bool>(nameof(HasRedirectIds));
            set => SetData(nameof(HasRedirectIds), value);
        }

        /// <summary>
        /// The request URI that should be replaced with <see cref="LocationUri"/>.
        /// </summary>
        public int HttpStatusCode
        {
            get => GetData<int>(nameof(HttpStatusCode));
            set => SetData(nameof(HttpStatusCode), value);
        }

        /// <summary>
        /// The request URI that should be replaced with <see cref="LocationUri"/>.
        /// </summary>
        public Uri RequestUri
        {
            get => GetData<Uri>(nameof(RequestUri));
            set => SetData(nameof(RequestUri), value);
        }

        /// <summary>
        /// The new URI that should replace <see cref="RequestUri"/>.
        /// </summary>
        public Uri LocationUri
        {
            get => GetData<Uri>(nameof(LocationUri));
            set => SetData(nameof(LocationUri), value);
        }

        /// <inheritdoc />
        public override string FriendlyMessage { get; set; } =
            "The request URI should be replace by the suggested URI.";

        private void SetProperties(Exception innerException = null)
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }

        private bool TryInterpretRedirectException(HttpResponseMessage response)
        {
            if (response.Headers.Location == null) return false;
            if (response.RequestMessage.RequestUri == null) return false;
            if (response.RequestMessage.RequestUri.Host != response.Headers.Location.Host) return false;
            var requestPath = WebUtility.UrlDecode(response.RequestMessage.RequestUri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(requestPath)) return false;
            var redirectPath = WebUtility.UrlDecode(response.Headers.Location.AbsolutePath);
            if (string.IsNullOrWhiteSpace(redirectPath)) return false;
            var differsAt = DiffersAtIndex(requestPath, redirectPath);
            // Same or totally different
            if (differsAt < 1) return false;
            var oldIdWithTail = RemoveHeadToMax(requestPath, differsAt);
            var newIdWithTail = RemoveHeadToMax(redirectPath, differsAt);
            var oldId = RemoveTail(oldIdWithTail);
            if (string.IsNullOrWhiteSpace(oldId)) return false;
            var newId = RemoveTail(newIdWithTail);
            if (string.IsNullOrWhiteSpace(newId)) return false;
            var oldTail = oldIdWithTail.Substring(oldId.Length);
            var newTail = newIdWithTail.Substring(newId.Length);
            if (oldTail != newTail) return false;
            FromId = oldId;
            ToId = newId;
            return true;

            int DiffersAtIndex(string s1, string s2)
            {
                int index = 0;
                int min = Math.Min(s1.Length, s2.Length);
                while (index < min && s1[index] == s2[index])
                    index++;

                return (index == min && s1.Length == s2.Length) ? -1 : index;
            }

            string RemoveHeadToMax(string s, int max)
            {
                for (int i = max; i >= 0; i--)
                {
                    var c = s[i];
                    if (c == '/' || c == '?')
                    {
                        return s.Substring(i + 1, s.Length - i - 1);
                    }
                }
                return s;
            }

            string RemoveTail(string s)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    var c = s[i];
                    if (c == '/' || c == '?') return s.Substring(0, i);
                }
                return s;
            }
        }
    }
}
