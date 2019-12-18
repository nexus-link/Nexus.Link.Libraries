using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.RestClientHelper;

#pragma warning disable 659

namespace Nexus.Link.Libraries.Web.Tests.RestClientHelper
{
    [TestClass]
    public class ReflectionTest
    { [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(ReflectionTest).FullName);
        }

        [TestMethod]
        public void StringFinder()
        {
            var foo = new Foo {Id = "1", Age=12, Name1 = "name"};
            var result = foo.JoinStrings();
            Assert.IsTrue(result.Contains($"Name1={foo.Name1};"));
            Assert.IsTrue(result.Contains($"Ids=,2,3;"));
        }

        private class Foo : IUniquelyIdentifiable<string>
        {
            public Foo()
            {
                When = DateTimeOffset.Now;
                Ids = new[] {"2", "3"};
            }

            /// <inheritdoc />
            [TranslationConcept("foo.id")]
            public string Id { get; set; }

            public string Name1 { get; set; }
            
            public string Name2 { get; set; }

            public DateTimeOffset When { get; }

            public int Age { get; set; }
            public string[] Ids { get; }

            public override string ToString() => Name1;
        }
    }
}
