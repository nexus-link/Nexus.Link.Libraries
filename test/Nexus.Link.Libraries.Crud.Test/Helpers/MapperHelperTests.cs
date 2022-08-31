using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Crud.Helpers;

#pragma warning disable 618

namespace Nexus.Link.Libraries.Crud.Test.Helpers
{
    [TestClass]
    public class MapperHelperTests
    {

        [TestInitialize]
        public void Initialize()
        {
        }

        #region ToGuid
        [TestMethod]
        public void MapToTypeGuid()
        {
            var source = Guid.NewGuid().ToString();
            var value = MapperHelper.MapToType<Guid, string>(source);
            Assert.AreEqual(source, value.ToString());
        }

        [TestMethod]
        public void StringToGuid()
        {
            var source = Guid.NewGuid().ToString();
            var value = source.ToGuid();
            Assert.AreEqual(source, value.ToString());
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("not a guid")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToTypeGuidFail(string source)
        {
            MapperHelper.MapToType<Guid, string>(source);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("not a guid")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void StringToGuidFail(string source)
        {
            source.ToGuid();
        }
        #endregion

        #region ToEnum
        [TestMethod]
        public void MapToStructEnum()
        {
            var source = TestEnum.Value1.ToString();
            var value = MapperHelper.MapToStruct<TestEnum, string>(source);
            Assert.AreEqual(source, value.ToString());
        }

        [TestMethod]
        public void StringToEnum()
        {
            var source = TestEnum.Value1.ToString();
            var value = source.ToEnum<TestEnum>();
            Assert.AreEqual(source, value.ToString());
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("illegal value")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToStructEnumFail(string source)
        {
            MapperHelper.MapToStruct<TestEnum, string>(source);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("illegal value")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void StringToEnumFail(string source)
        {
            source.ToEnum<TestEnum>();
        }
        #endregion

        #region ToGuid?
        [TestMethod]
        public void MapToTypeGuid_Nullable()
        {
            var source = (string) null;
            var value = MapperHelper.MapToType<Guid?, string>(source);
            Assert.IsNull(value);
        }

        [DataTestMethod]
        [DataRow("not a guid")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToTypeGuidFail_Nullable(string source)
        {
            MapperHelper.MapToType<Guid?, string>(source);
        }
        #endregion

        #region ToEnum?
        [TestMethod]
        public void MapToStructEnum_Nullable()
        {
            var value = MapperHelper.MapToStructOrNull<TestEnum, string>((string)null);
            Assert.IsNull(value);
        }

        [DataTestMethod]
        [DataRow("illegal value")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToStructEnumFail_Nullable(string source)
        {
            MapperHelper.MapToStructOrNull<TestEnum, string>(source);
        }
        #endregion
    }

    internal enum TestEnum
    {
        Value1,
        Value2
    };
}
