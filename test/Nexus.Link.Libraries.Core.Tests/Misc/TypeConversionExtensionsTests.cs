using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Core.Tests.Misc
{
    [TestClass]
    public class TypeConversionExtensionsTests
    {

        [TestInitialize]
        public void Initialize()
        {
        }

        #region ToGuid
        [TestMethod]
        public void ToGuid()
        {
            var source = Guid.NewGuid().ToString();
            var value = source.ToGuid();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(source, value.ToString());
        }
        [TestMethod]
        public void MapToTypeGuid()
        {
            var source = Guid.NewGuid().ToString();
            var value = TypeConversionExtensions.MapToType<Guid, string>(source);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(source, value.ToString());
        }

        [TestMethod]
        public void StringToGuid()
        {
            var source = Guid.NewGuid().ToString();
            var value = source.ToGuid();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(source, value.ToString());
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("not a guid")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToTypeGuidFail(string source)
        {
            TypeConversionExtensions.MapToType<Guid, string>(source);
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
            var value = TypeConversionExtensions.MapToStruct<TestEnum, string>(source);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(source, value.ToString());
        }

        [TestMethod]
        public void StringToEnum()
        {
            var source = TestEnum.Value1.ToString();
            var value = source.ToEnum<TestEnum>();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(source, value.ToString());
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("illegal value")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToStructEnumFail(string source)
        {
            TypeConversionExtensions.MapToStruct<TestEnum, string>(source);
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
            var value = TypeConversionExtensions.MapToType<Guid?, string>(source);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(value);
        }

        [DataTestMethod]
        [DataRow("not a guid")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToTypeGuidFail_Nullable(string source)
        {
            TypeConversionExtensions.MapToType<Guid?, string>(source);
        }
        #endregion

        #region ToEnum?
        [TestMethod]
        public void MapToStructEnum_Nullable()
        {
            var source = (string)null;
            var value = TypeConversionExtensions.MapToStructOrNull<TestEnum, string>(source);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNull(value);
        }

        [DataTestMethod]
        [DataRow("illegal value")]
        [ExpectedException(typeof(FulcrumContractException))]
        public void MapToStructEnumFail_Nullable(string source)
        {
            TypeConversionExtensions.MapToStructOrNull<TestEnum, string>(source);
        }
        #endregion
    }

    internal enum TestEnum
    {
        Value1,
        Value2
    };
}
