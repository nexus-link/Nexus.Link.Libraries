﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.MultiTenant.Model;
using Nexus.Link.Libraries.Core.Platform.Configurations;
using Nexus.Link.Libraries.Web.Pipe.Outbound;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Web.ServiceAuthentication
{
    public class ServiceAuthenticationHelper : IServiceAuthenticationHelper
    {
        private const string CacheName = "ServiceAuthenticationHelper.Cache";
        private static MemoryCache _authCache = new MemoryCache(CacheName);

        /// <summary>
        /// The HttpClient that is used for all HTTP calls.
        /// </summary>
        /// <remarks>Is set to <see cref="HttpClient"/> by default. Typically only set to other values for unit test purposes.</remarks>
        public static IHttpClient HttpClient { get; set; }

        private static readonly object LockClass = new object();

        public ServiceAuthenticationHelper()
        {
            lock (LockClass)
            {
                if (HttpClient != null) return;
                var handlers = OutboundPipeFactory.CreateDelegatingHandlers();
                var httpClient = HttpClientFactory.Create(handlers);
                HttpClient = new HttpClientWrapper(httpClient);
            }
        }

        /// <inheritdoc />
        public async Task<AuthorizationToken> GetAuthorizationForClientAsync(Tenant tenant, ILeverConfiguration configuration, string client)
        {
            var cacheKey = $"{tenant}/{client}";
            var authorization = _authCache[cacheKey] as AuthorizationToken;
            if (authorization == null)
            {
                ClientAuthorizationSettings authSettings = null;
                try
                {
                    // Note: We can't just use configuration.Value<ClientAuthorizationSettings>($"{client}-authentication")
                    // because we get exception "Cannot cast Newtonsoft.Json.Linq.JObject to Newtonsoft.Json.Linq.JToken".
                    // See ServiceAuthenticationHelperTest.ShowWhyWeHaveToMakeWorkaroundInServiceAuthenticationHelper
                    var tenantClientSetting = configuration?.Value<JObject>($"{client}-authentication");
                    if (tenantClientSetting == null)
                    {
                        var shared = configuration?.Value<JArray>("shared-client-authentications");
                        if (shared != null)
                        {
                            var sharedSettings = JsonConvert.DeserializeObject<List<ClientAuthorizationSettings>>(shared.ToString());
                            var setting = sharedSettings?.FirstOrDefault(x => x.UseForClients.Contains(client));
                            if (setting != null) tenantClientSetting = JObject.FromObject(setting);
                        }
                    }
                    if (tenantClientSetting == null)
                    {
                        tenantClientSetting = JObject.FromObject(new ClientAuthorizationSettings { AuthorizationType = ClientAuthorizationSettings.AuthorizationTypeEnum.None });
                    }

                    authSettings = JsonConvert.DeserializeObject<ClientAuthorizationSettings>(tenantClientSetting.ToString());
                    FulcrumAssert.IsNotNull(authSettings, null, "Expected non-null auth settings");
                    FulcrumAssert.IsNotNull(authSettings.AuthorizationType, null, "Expected AuthorizationType");

                    string token, tokenType;
                    switch (authSettings.AuthorizationType)
                    {
                        case ClientAuthorizationSettings.AuthorizationTypeEnum.None:
                            return null;
                        case ClientAuthorizationSettings.AuthorizationTypeEnum.Basic:
                            FulcrumAssert.IsNotNullOrWhiteSpace(authSettings.Username, null,
                                "Expected a Basic Auth Username");
                            FulcrumAssert.IsNotNull(authSettings.Password, null, "Expected a Basic Auth Password");
                            token = Base64Encode($"{authSettings.Username}:{authSettings.Password}");
                            tokenType = "Basic";
                            break;
                        case ClientAuthorizationSettings.AuthorizationTypeEnum.BearerToken:
                            FulcrumAssert.IsNotNullOrWhiteSpace(authSettings.Token, "Expected a Bearer token");
                            token = authSettings.Token;
                            tokenType = "Bearer";
                            break;
                        case ClientAuthorizationSettings.AuthorizationTypeEnum.JwtFromUrl:
                            token = await FetchJwtFromUrl(authSettings);
                            tokenType = "Bearer";
                            break;
                        default:
                            throw new ArgumentException($"Unknown Authorization Type: '{authSettings.AuthorizationType}'");
                    }

                    authorization = new AuthorizationToken { Type = tokenType, Token = token };
                    if (token != null)
                    {
                        _authCache.Set(cacheKey, authorization, DateTimeOffset.Now.AddMinutes(authSettings.TokenCacheInMinutes));
                    }
                }
                catch (Exception e)
                {
                    Log.LogError($"Could not handle Authentication for client '{client}' in tenant '{tenant}'. Settings: '{authSettings}'", e);
                    throw;
                }
            }

            return authorization;
        }

        private async Task<string> FetchJwtFromUrl(ClientAuthorizationSettings authSettings)
        {
            FulcrumAssert.IsNotNullOrWhiteSpace(authSettings.PostUrl, null, "Expected a Url to Post to");
            FulcrumAssert.IsNotNullOrWhiteSpace(authSettings.PostBody, null, "Expected a Body to post to the url");
            FulcrumAssert.IsNotNullOrWhiteSpace(authSettings.ResponseTokenJsonPath, null, "Expected a Json path to the token in the response");

            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(authSettings.PostUrl),
                Content = new StringContent(authSettings.PostBody, Encoding.UTF8, "application/json")
            };

            var response = "";
            try
            {
                var httpResponse = await HttpClient.SendAsync(httpRequest, CancellationToken.None);
                response = await httpResponse.Content.ReadAsStringAsync();
                if (!httpResponse.IsSuccessStatusCode)
                {
                    Log.LogError($"Response not successful. Code {httpResponse.StatusCode}");
                    return null;
                }

                var result = JToken.Parse(response);
                var jwtToken = result.SelectToken(authSettings.ResponseTokenJsonPath).ToObject<string>();
                return jwtToken;
            }
            catch (Exception e)
            {
                Log.LogError(
                    $"Error fetching token from '{authSettings.PostUrl}' with json path '{authSettings.ResponseTokenJsonPath} on response '{response}'. Error message: {e.Message}", e);
                return null;
            }
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static void ClearCache()
        {
            _authCache = new MemoryCache(CacheName);
        }
    }
}