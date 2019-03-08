namespace Nexus.Link.Libraries.Web.Tests
{
  /*  [TestClass]
    public class ContextTests
    {
        private static void MultipleSetValue(IValueProvider valueProvider)
        {
            const string key = "key";
            valueProvider.SetValue(key, "value1");
            valueProvider.SetValue(key, "value2");
            Assert.AreEqual("value2", valueProvider.GetValue<string>(key));
        }

        [TestMethod]
        public void AsyncLocalValueProviderMultipleSetValue()
        {
            HttpContext.Current = FakeHttpContext();

            MultipleSetValue(new AsyncLocalValueProvider());
        }

        [TestMethod]
        public void HttpContextValueProviderMultipleSetValue()
        {
            HttpContext.Current = FakeHttpContext();
            MultipleSetValue(new HttpContextValueProvider());
        }

        [TestMethod]
        public void HttpContextValueProviderMultipleSetValueFallback()
        {
            // Note! HttpContext.Current needs to be null
            MultipleSetValue(new HttpContextValueProvider());
        }

        public static HttpContext FakeHttpContext()
        {
            var httpRequest = new HttpRequest("", "http://stackoverflow/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                                                    new HttpStaticObjectsCollection(), 10, true,
                                                    HttpCookieMode.AutoDetect,
                                                    SessionStateMode.InProc, false);

            httpContext.Items["AspSession"] = typeof(HttpSessionState).GetConstructor(
                                        BindingFlags.NonPublic | BindingFlags.Instance,
                                        null, CallingConventions.Standard,
                                        new[] { typeof(HttpSessionStateContainer) },
                                        null)?
                                .Invoke(new object[] { sessionContainer });

            return httpContext;
        }

    }*/
}
