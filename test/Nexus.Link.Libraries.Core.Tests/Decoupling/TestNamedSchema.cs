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
    public class TestNamedSchema
    {
        [TestMethod]
        public void SchemaNoName()
        {
            var dataBefore = new DataAnonymous { Zero = Guid.NewGuid().ToString() };
            var json = JsonConvert.SerializeObject(dataBefore);
            var probe = JsonConvert.DeserializeObject<NamedSchema>(json);
            UT.Assert.IsNotNull(probe);
            UT.Assert.IsNull(probe.SchemaName);
            UT.Assert.AreEqual(0, probe.SchemaVersion);
        }

        [TestMethod]
        public void SchemaVersioned()
        {
            var dataBefore = new DataType1Version1 { First = Guid.NewGuid().ToString() };
            var json = JsonConvert.SerializeObject(dataBefore);
            var probe = JsonConvert.DeserializeObject<NamedSchema>(json);
            UT.Assert.IsNotNull(probe);
            UT.Assert.IsNotNull(probe.SchemaVersion);
            UT.Assert.AreEqual(dataBefore.SchemaName, probe.SchemaName);
            UT.Assert.AreEqual(dataBefore.SchemaVersion, probe.SchemaVersion);
        }

        [TestMethod]
        public void DeserializationBasedOnSchemaVersion()
        {
            var dataOfDifferentVersions = new List<object>()
            {
                new DataAnonymous { Zero = Guid.NewGuid().ToString() },
                new DataType1Version1 { First = Guid.NewGuid().ToString() },
                new DataType2Version2 { Second = Guid.NewGuid().ToString() }
            };
            foreach (var dataBefore in dataOfDifferentVersions)
            {
                var json = JsonConvert.SerializeObject(dataBefore);
                var probe = JsonConvert.DeserializeObject<NamedSchema>(json);
                UT.Assert.IsNotNull(probe);
                if (probe.SchemaName == null)
                {
                    var dataAfter = JsonConvert.DeserializeObject<DataAnonymous>(json);
                    UT.Assert.AreEqual(dataBefore, dataAfter);
                }
                else
                {
                    switch (probe.SchemaName)
                    {
                        case "DataType1":
                            var dataAfter1 = JsonConvert.DeserializeObject<DataType1Version1>(json);
                            UT.Assert.AreEqual(dataBefore, dataAfter1);
                            break;
                        case "DataType2":
                            var dataAfter2 = JsonConvert.DeserializeObject<DataType2Version2>(json);
                            UT.Assert.AreEqual(dataBefore, dataAfter2);
                            break;
                        default:
                            UT.Assert.Fail($"Unknown schema name: {probe.SchemaName}");
                            break;
                    }
                }
            }
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
            public int SchemaVersion { get; } = 1;
            public string First { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataType1Version1 data)) return false;
                return data.First == First;
            }
        }

        private class DataType2Version2 : INamedSchema
        {
            public string SchemaName { get; } = "DataType2";
            public int SchemaVersion { get; } = 2;
            public string Second { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is DataType2Version2 data)) return false;
                return data.Second == Second;
            }
        }
    }
}
