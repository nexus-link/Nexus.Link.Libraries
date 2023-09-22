using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Storage.Logic;
using Nexus.Link.Libraries.Core.Storage.Model;
using Shouldly;

namespace Nexus.Link.Libraries.Core.Tests.Storage
{
    [TestClass]
    public class StorageHelperTests
    {

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public async Task ReadPages_Given_TestEqualsDuplicates_Gives_DuplicatesRemoved()
        {
            // Arrange

            // Act
            var result = await StorageHelper.ReadPagesAsync(ReturnPagesAsync<TestEquals>, int.MaxValue);

            // Assert
            result.Count().ShouldBe(100);
        }

        [TestMethod]
        public async Task ReadPages_Given_TestUniquelyIdentifiableDuplicates_Gives_DuplicatesRemoved()
        {
            // Arrange

            // Act
            var result = await StorageHelper.ReadPagesAsync(ReturnPagesAsync<TestUniquelyIdentifiable>, int.MaxValue);

            // Assert
            result.Count().ShouldBe(100);
        }

        private async Task<PageEnvelope<T>> ReturnPagesAsync<T>(int offset, CancellationToken token) where T : TestItem, new()
        {
            TestItem.Index = 0;
            var list = new List<T>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(new T());
            }

            await Task.CompletedTask;
            const int pageSize = 5;
            var failOffset = offset - offset / pageSize;
            if (failOffset < 0) failOffset = offset;
            var testItems = list.Skip(failOffset).Take(pageSize);
            return new PageEnvelope<T>(offset, pageSize, null,
                testItems);
        }
    }

    internal class TestUniquelyIdentifiable : TestItem, IUniquelyIdentifiable<int>
    {
    }

    internal class TestEquals : TestItem
    {
        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is TestEquals item)) return false;
            return Id == item.Id;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    internal class TestItem
    {
        public static int Index { get; set; }
        public TestItem()
        {
            Id = Index++;
            Content = Guid.NewGuid();
        }

        public int Id { get; set; }

        public Guid Content { get; set; }
    }
}
