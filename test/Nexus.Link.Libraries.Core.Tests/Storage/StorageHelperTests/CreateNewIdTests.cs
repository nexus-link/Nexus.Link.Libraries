using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;
using Shouldly;

namespace Nexus.Link.Libraries.Core.Tests.Storage.StorageHelperTests
{
    [TestClass]
    public class CreateNewIdTests
    {
        [TestMethod]
        [DataRow(GuidOptimization.None)]
        [DataRow(GuidOptimization.SqlServer)]
        [DataRow(GuidOptimization.SqlServerWithProcessId)]
        [DataRow(GuidOptimization.MySql)]
        [DataRow(GuidOptimization.RavenDb)]
        [DataRow(GuidOptimization.Oracle)]
        public void CreateNewId_Guid_Given_Optimization_Creates_NewGuid(GuidOptimization optimization)
        {
            // Arrange
            StorageHelper.Optimization = optimization;

            // Act
            var result = StorageHelper.CreateNewId<Guid>();

            // Assert
            result.ShouldNotBe(default);
        }

        [TestMethod]
        [DataRow(GuidOptimization.None)]
        [DataRow(GuidOptimization.SqlServer)]
        [DataRow(GuidOptimization.SqlServerWithProcessId)]
        [DataRow(GuidOptimization.MySql)]
        [DataRow(GuidOptimization.RavenDb)]
        [DataRow(GuidOptimization.Oracle)]
        public void CreateNewId_String_Given_Optimization_Creates_NewString(GuidOptimization optimization)
        {
            // Arrange
            StorageHelper.Optimization = optimization;

            // Act
            var result = StorageHelper.CreateNewId<string>();

            // Assert
            result.ShouldNotBe(default);
            Guid.TryParse(result, out var _).ShouldBeTrue();
        }
    }
}