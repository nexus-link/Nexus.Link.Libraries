using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Error.Model;
using System;
using System.Net;
using JetBrains.Annotations;
using System.Net.Http;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using System.Linq;

namespace Nexus.Link.Libraries.Core.Error.Logic
{
    /// <summary>
    /// The request resulted in an HTTP status code in the 3xx range. We will return the Uri for the request and the location header.
    /// </summary>
    public class FulcrumHttpRedirectException : FulcrumException
    {
        /// <summary>
        /// Factory method
        /// </summary>
        public static FulcrumException Create(string message, Exception innerException = null)
        {
            return new FulcrumHttpRedirectException(message, innerException);
        }

        /// <summary>
        /// The type for this <see cref="FulcrumException"/>
        /// </summary>
        public const string ExceptionType = "Xlent.Fulcrum.Http.Redirect";

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
        public FulcrumHttpRedirectException(HttpResponseMessage response, string content) : this(
            $"Redirect from {response?.RequestMessage?.RequestUri} to {response?.Headers?.Location}.", (Exception)null)
        {
            InternalContract.RequireNotNull(response, nameof(response));
            InternalContract.RequireGreaterThanOrEqualTo(300, (int)response.StatusCode, $"{nameof(response)}.{nameof(response.StatusCode)}");
            InternalContract.RequireLessThan(400, (int)response.StatusCode, $"{nameof(response)}.{nameof(response.StatusCode)}");

            RequestUri = response.RequestMessage?.RequestUri;
            LocationUri = response.Headers?.Location;
            HttpStatusCode = (int)response.StatusCode;
            Content = content;
            ContentType = response.Content?.Headers?.ContentType?.MediaType;
            // If this call returns true, then OldId and NewId has been set.
            HasRedirectIds = TryInterpretRedirectException(RequestUri, LocationUri);
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
        /// The HTTP status code from the response
        /// </summary>
        public int HttpStatusCode
        {
            get => GetData<int>(nameof(HttpStatusCode));
            set => SetData(nameof(HttpStatusCode), value);
        }

        /// <summary>
        /// The HTTP Content-Type from the response
        /// </summary>
        public string ContentType
        {
            get => GetData<string>(nameof(ContentType));
            set => SetData(nameof(ContentType), value);
        }

        /// <summary>
        /// The content from the response
        /// </summary>
        public string Content
        {
            get => GetData<string>(nameof(Content));
            set => SetData(nameof(Content), value);
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

        /// <summary>
        /// The request URI that should be replaced with <see cref="LocationUri"/>.
        /// </summary>
        public bool HasRedirectIds
        {
            get => GetData<bool>(nameof(HasRedirectIds));
            set => SetData(nameof(HasRedirectIds), value);
        }

        /// <summary>
        /// The old id that should be replaced with <see cref="NewId"/>.
        /// </summary>
        public string OldId
        {
            get => GetData<string>(nameof(OldId));
            set => SetData(nameof(OldId), value);
        }

        /// <summary>
        /// The new id that should replace <see cref="OldId"/>.
        /// </summary>
        public string NewId
        {
            get => GetData<string>(nameof(NewId));
            set => SetData(nameof(NewId), value);
        }

        /// <inheritdoc />
        public override string FriendlyMessage { get; set; } =
            "The request URI should be replace by the suggested URI.";

        private void SetProperties(Exception innerException = null)
        {
            MoreInfoUrl = $"http://lever.xlent-fulcrum.info/FulcrumExceptions#{Type}";
        }

        private bool TryInterpretRedirectException(Uri oldUri, Uri newUri)
        {
            if (oldUri == null || newUri == null) return false;
            if (oldUri.Host != newUri.Host) return false;

            // Get the path after host
            var oldPath = WebUtility.UrlDecode(oldUri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(oldPath)) return false;
            var newPath = WebUtility.UrlDecode(newUri.AbsolutePath);
            if (string.IsNullOrWhiteSpace(newPath)) return false;

            // Find the point where they differ
            var differsAt = DiffersAtIndex(oldPath, newPath);
            // Same or totally different
            if (differsAt < 1) return false;

            // Go backwards from the difference to find where the id starts
            var idStart = GetIdStartPosition(oldPath, differsAt);
            if (idStart > differsAt) return false;

            // Go forward from the difference to find where the id ends
            var oldIdEnd = GetIdEndPosition(oldPath, differsAt);
            if (oldIdEnd < differsAt) return false;
            var newIdEnd = GetIdEndPosition(newPath, differsAt);
            if (newIdEnd < differsAt) return false;

            // Verify that they still end with the same string
            var oldTail = oldPath.Substring(oldIdEnd + 1);
            var newTail = newPath.Substring(newIdEnd + 1);
            if (oldTail != newTail) return false;

            // Now extract the oldId and the newId
            OldId = oldPath.Substring(idStart, oldIdEnd - idStart + 1);
            FulcrumAssert.IsNotNullOrWhiteSpace(OldId, CodeLocation.AsString());
            NewId = newPath.Substring(idStart, oldIdEnd - idStart + 1);
            FulcrumAssert.IsNotNullOrWhiteSpace(NewId, CodeLocation.AsString());
            return true;

            int DiffersAtIndex(string s1, string s2)
            {
                var index = 0;
                var min = Math.Min(s1.Length, s2.Length);
                while (index < min && s1[index] == s2[index]) index++;
                return (index == min && s1.Length == s2.Length) ? -1 : index;
            }

            int GetIdStartPosition(string s, int max)
            {
                for (var i = max; i >= 0; i--)
                {
                    var c = s[i];
                    if (c is '/' or '?' or ' ') return i + 1;
                }
                return 0;
            }

            int GetIdEndPosition(string s, int min)
            {
                for (var i = min; i < s.Length; i++)
                {
                    var c = s[i];
                    if (c is '/' or '?' or ' ') return i - 1;
                }
                return s.Length - 1;
            }
        }
    }
}
