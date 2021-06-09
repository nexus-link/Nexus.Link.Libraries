using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Translation;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Nexus.Link.Libraries.Core.Tests.Translate
{
    [TestClass]
    public class TestTranslator
    {
        private Mock<ITranslatorService> _translatorServiceMock;
        private static readonly string _consumerId = "in-1";
        private static readonly string _producerId = "out-1";
        private static string _decoratedConsumerId;
        private ITranslator _consumerTranslator;

        [TestInitialize]
        public void Initialize()
        {
            _decoratedConsumerId = $"(id!~consumer!{_consumerId})";
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestTranslator).FullName);
            _translatorServiceMock = new Mock<ITranslatorService>();
            _translatorServiceMock
                .Setup(ts => ts.TranslateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Dictionary<string, string> {{$"{_decoratedConsumerId}", _producerId}});
            _consumerTranslator = new Translator("consumer", _translatorServiceMock.Object);
        }

        [TestMethod]
        public void DecorateObjectWithString()
        {
            var item = new ObjectWithString();
            var decoratedItem = _consumerTranslator.Decorate(item);
            decoratedItem.VerifyAfterDecoration();
        }

        [TestMethod]
        public void DecorateListOfObjectWithString()
        {
            var items = new List<ObjectWithString>();
            for (var i = 0; i < 2; i++)
            {
                var item = new ObjectWithString();
                items.Add(item);
            }
            var decoratedItems = _consumerTranslator.Decorate(items);
            foreach (var decoratedItem in decoratedItems)
            {
                decoratedItem.VerifyAfterDecoration();
            }
        }

        [TestMethod]
        public void DecorateObjectWithList()
        {
            var item = new ObjectWithList();
            var decoratedItem = _consumerTranslator.Decorate(item);
            decoratedItem.VerifyAfterDecoration();
        }

        [TestMethod]
        public void DecorateObjectWithArray()
        {
            var item = new ObjectWithArray();
            var decoratedItem = _consumerTranslator.Decorate(item);
            decoratedItem.VerifyAfterDecoration();
        }

        [TestMethod]
        public void DecorateObjectWithVariousTypes()
        {
            var item = new ObjectWithVariousTypes();
            var decoratedItem = _consumerTranslator.Decorate(item);
            decoratedItem.VerifyAfterDecoration();
        }

        private class ObjectWithString
        {
            [TranslationConcept("id")] public string Id { get; set; } = _consumerId;
            public void VerifyAfterDecoration()
            {
                UT.Assert.AreEqual(_decoratedConsumerId, Id);
            }
        }

        private class ObjectWithList
        {
            [TranslationConcept("id")] 
            public List<string> IdList { get; set; } = new List<string> {_consumerId, _consumerId};
            public void VerifyAfterDecoration()
            {
                foreach (var id in IdList)
                {
                    UT.Assert.AreEqual(_decoratedConsumerId, id);
                }
            }
        }

        private class ObjectWithArray
        {
            [TranslationConcept("id")] 
            public string[] IdArray { get; set; } = {_consumerId, _consumerId};
            public void VerifyAfterDecoration()
            {
                foreach (var id in IdArray)
                {
                    UT.Assert.AreEqual(_decoratedConsumerId, id);
                }
            }
        }

        private class ObjectWithVariousTypes
        {
            [TranslationConcept("id")] public string Id { get; set; } = _consumerId;

            public string Name { get; set; } = "not-to-be-decorated";

            public DateTime NotInitializedDateTime { get; set; }

            public DateTime InitializedDateTime { get; set; } = DateTime.Now;

            public TimeSpan NotInitializedTimeSpan { get; set; }

            public TimeSpan InitializedTimeSpan { get; set; } = TimeSpan.Zero;

            public int Integer { get; set; } = 5;

            public dynamic DynamicObject { get; set; } = new {String = "23", Integer = 23, DateTime = DateTime.Now};

            public void VerifyAfterDecoration()
            {
                UT.Assert.AreEqual(_decoratedConsumerId, Id);
                UT.Assert.AreEqual("not-to-be-decorated", Name);
                UT.Assert.AreEqual("23", DynamicObject.String);
                UT.Assert.AreEqual(23, DynamicObject.Integer);
            }
        }
    }
}
