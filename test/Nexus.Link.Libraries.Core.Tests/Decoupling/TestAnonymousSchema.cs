using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Decoupling;
using UT = Microsoft.VisualStudio.TestTools.UnitTesting;
#pragma warning disable 659

namespace Nexus.Link.Libraries.Core.Tests.Decoupling
{
    [TestClass]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    public class TestAnonymousSchema
    {
        [TestMethod]
        public void SchemaNotVersioned()
        {
            var dataBefore = new DataNoVersion { Zero = Guid.NewGuid().ToString() };
            var json = JsonConvert.SerializeObject(dataBefore);
            var probe = JsonConvert.DeserializeObject<AnonymousSchema>(json);
            UT.Assert.IsNotNull(probe);
            UT.Assert.AreEqual(0, probe.SchemaVersion);
        }

        [TestMethod]
        public void SchemaVersioned()
        {
            var dataBefore = new DataVersion1 { First = Guid.NewGuid().ToString() };
            var json = JsonConvert.SerializeObject(dataBefore);
            var probe = JsonConvert.DeserializeObject<AnonymousSchema>(json);
            UT.Assert.IsNotNull(probe);
            UT.Assert.AreEqual(dataBefore.SchemaVersion, probe.SchemaVersion);
        }

        [TestMethod]
        public void DeserializationBasedOnSchemaVersion()
        {
            var dataOfDifferentVersions = new List<object>()
            {
                new DataNoVersion { Zero = Guid.NewGuid().ToString() },
                new DataVersion1 { First = Guid.NewGuid().ToString() },
                new DataVersion2 { Second = Guid.NewGuid().ToString() }
            };
            foreach (var dataBefore in dataOfDifferentVersions)
            {
                var json = JsonConvert.SerializeObject(dataBefore);
                var probe = JsonConvert.DeserializeObject<AnonymousSchema>(json);
                UT.Assert.IsNotNull(probe);
                switch (probe.SchemaVersion)
                {
                    case 0:
                        var dataAfter = JsonConvert.DeserializeObject<DataNoVersion>(json);
                        UT.Assert.AreEqual(dataBefore, dataAfter);
                        break;
                    case 1:
                        var dataAfter1 = JsonConvert.DeserializeObject<DataVersion1>(json);
                        UT.Assert.AreEqual(dataBefore, dataAfter1);
                        break;
                    case 2:
                        var dataAfter2 = JsonConvert.DeserializeObject<DataVersion2>(json);
                        UT.Assert.AreEqual(dataBefore, dataAfter2);
                        break;
                    default:
                        UT.Assert.Fail($"Unknown schema version: {probe.SchemaVersion}");
                        break;
                }
            }
        }


        private class DataNoVersion
        {
            public string Zero { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataNoVersion data)) return false;
                return data.Zero == Zero;
            }
        }

        private class DataVersion1 : IVersionedSchema
        {
            public int SchemaVersion { get; } = 1;
            public string First { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataVersion1 data)) return false;
                return data.First == First;
            }
        }

        private class DataVersion2 : IVersionedSchema
        {
            public int SchemaVersion { get; } = 2;
            public string Second { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataVersion2 data)) return false;
                return data.Second == Second;
            }
        }
    }
}
