using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Decoupling;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Tests.Queue;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;
#pragma warning disable 659

namespace Nexus.Link.Libraries.Core.Tests.Decoupling
{
    [TestClass]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    public class TestSchemaParser
    {
        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(TestSchemaParser).FullName);
        }

        [TestMethod]
        public void ParseTypes()
        {
            var schemaParser = new SchemaParser()
                .Add(typeof(DataAnonymous))
                .Add("DataType1", 1, typeof(DataType1Version1))
                .Add("DataType2", 1, typeof(DataType2Version1))
                .Add("DataType2", 2, typeof(DataType2Version2));
            var dataOfDifferentVersions = new List<object>()
            {
                new DataAnonymous {Zero = Guid.NewGuid().ToString()},
                new DataType1Version1 {First = Guid.NewGuid().ToString()},
                new DataType2Version1 {Second = Guid.NewGuid().ToString()},
                new DataType2Version2 {Third = Guid.NewGuid().ToString()}
            };
            foreach (var dataBefore in dataOfDifferentVersions)
            {
                var json = JsonConvert.SerializeObject(dataBefore);
                var success = schemaParser.TryParse(json, out _, out _, out var dataAfter);
                UT.Assert.IsTrue(success);
                UT.Assert.AreEqual(dataBefore, dataAfter);
            }
        }

        [TestMethod]
        public void UnknownType()
        {
            var schemaParser = new SchemaParser()
                .Add("DataType1", 1, typeof(DataType1Version1));
            var dataBefore = new DataType2Version2 { Third = Guid.NewGuid().ToString() };
            var json = JsonConvert.SerializeObject(dataBefore);
            var success = schemaParser.TryParse(json, out var schemaName, out var schemaVersion, out _);
            UT.Assert.IsFalse(success);
            UT.Assert.AreEqual("DataType2", schemaName);
            UT.Assert.AreEqual(2, schemaVersion);
        }

        [TestMethod]
        public void UnknownVersion()
        {
            var schemaParser = new SchemaParser()
                .Add("DataType2", 1, typeof(DataType2Version1));
            var dataBefore = new DataType2Version2 { Third = Guid.NewGuid().ToString() };
            var json = JsonConvert.SerializeObject(dataBefore);
            var success = schemaParser.TryParse(json, out var schemaName, out var schemaVersion, out _);
            UT.Assert.IsFalse(success);
            UT.Assert.AreEqual("DataType2", schemaName);
            UT.Assert.AreEqual(2, schemaVersion);
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public void NegativeVersion()
        {
            new SchemaParser().Add(-1, typeof(DataAnonymous));
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public void NullName()
        {
            new SchemaParser().Add(null, 1, typeof(DataAnonymous));
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumContractException))]
        public void NullType()
        {
            new SchemaParser().Add("Test", 1, null);
        }


        private class DataAnonymous
        {
            public string Zero { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataAnonymous data)) return false;
                return data.Zero == Zero;
            }
        }

        private class DataType1Version1 : INamedSchema
        {
            public string SchemaName { get; } = "DataType1";
            public int? SchemaVersion { get; } = 1;
            public string First { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataType1Version1 data)) return false;
                return data.First == First;
            }
        }

        private class DataType2Version1 : INamedSchema
        {
            public string SchemaName { get; } = "DataType2";
            public int? SchemaVersion { get; } = 1;
            public string Second { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataType2Version1 data)) return false;
                return data.Second == Second;
            }
        }

        private class DataType2Version2 : INamedSchema
        {
            public string SchemaName { get; } = "DataType2";
            public int? SchemaVersion { get; } = 2;
            public string Third { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataType2Version2 data)) return false;
                return data.Third == Third;
            }
        }
    }
}
