using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;
using Shouldly;

namespace Nexus.Link.Libraries.Core.Tests.Storage.StorageHelperTests
{
    [TestClass]
    public class GuidGeneratorTests
    {
        [TestMethod]
        public void Optimization_None_IGuidGenerator_ShouldBeNull()
        {
            // Arrange
            StorageHelper.Optimization = GuidOptimization.None;

            // Act
            var sut = StorageHelper.GuidGenerator;

            // Assert
            sut.ShouldBeNull();
        }

        [TestMethod]
        public void Optimization_SqlServer_IGuidGenerator_ShouldBe_SqlServerGuidGenerator()
        {
            // Arrange
            StorageHelper.Optimization = GuidOptimization.SqlServer;

            // Act
            var sut = StorageHelper.GuidGenerator;

            // Assert
            sut.ShouldBeAssignableTo<SqlServerGuidGenerator>();
        }

        [TestMethod]
        public void Optimization_SqlServerWithProcessId_IGuidGenerator_ShouldBe_SqlServerGuidGenerator()
        {
            // Arrange
            StorageHelper.Optimization = GuidOptimization.SqlServerWithProcessId;

            // Act
            var sut = StorageHelper.GuidGenerator;

            // Assert
            sut.ShouldBeAssignableTo<SqlServerGuidGenerator>();
        }

        [TestMethod]
        [DataRow(GuidOptimization.MySql)]
        [DataRow(GuidOptimization.RavenDb)]
        public void Optimization_MySql_IGuidGenerator_ShouldBe_SqlServerGuidGenerator(GuidOptimization optimization)
        {
            // Arrange
            StorageHelper.Optimization = optimization;

            // Act
            var sut = StorageHelper.GuidGenerator;

            // Assert
            sut.ShouldBeAssignableTo<GenericGuidGenerator>();
            var guidGenerator = sut as GenericGuidGenerator;
            guidGenerator.SequentialGuidType.ShouldBe(SequentialGuidType.AsString);
        }

        [TestMethod]
        public void Optimization_Oracle_IGuidGenerator_ShouldBe_GenericGuidGenerator_SequentialGuidTypeAsBinary()
        {
            // Arrange
            StorageHelper.Optimization = GuidOptimization.Oracle;

            // Act
            var sut = StorageHelper.GuidGenerator;

            // Assert
            sut.ShouldBeAssignableTo<GenericGuidGenerator>();
            var guidGenerator = sut as GenericGuidGenerator;
            guidGenerator.SequentialGuidType.ShouldBe(SequentialGuidType.AsBinary);
        }
    }
}