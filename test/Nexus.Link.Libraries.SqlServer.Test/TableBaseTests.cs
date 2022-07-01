#if NETCOREAPP
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Core.Threads;
using Nexus.Link.Libraries.Crud.UnitTests.Model;
using Nexus.Link.Libraries.SqlServer.Logic;
using Nexus.Link.Libraries.SqlServer.Model;
using IsolationLevel = System.Transactions.IsolationLevel;

namespace Nexus.Link.Libraries.SqlServer.Test
{
    [TestClass]
    public class TableBaseTests
    {
        private CrudSql<TestItemBare, TestItemId<Guid>> _table;

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(CrudTestId));
            var connectionString = TestSettings.ConnectionString;
            FulcrumAssert.IsNotNullOrWhiteSpace(connectionString);
            var tableMetadata = new SqlTableMetadata
            {
                TableName = "TestItem",
                CustomColumnNames = new[] { "Value" },
                OrderBy = new string[] { }
            };
            var options = new DatabaseOptions
            {
                ConnectionString = connectionString,
                DefaultLockTimeSpan = TimeSpan.FromSeconds(30)
            };
            options.DistributedLockTable = new DistributedLockTable(options);
            _table = new CrudSql<TestItemBare, TestItemId<Guid>>(options, tableMetadata);
        }

        [TestMethod]
        public async Task ClaimLock_Given_ItemExists_Gives_ReturnItem()
        {
            var createItem = new TestItemBare
            {
                Value = "x"
            };
            var item = await _table.CreateAndReturnAsync(createItem);
            Assert.IsNotNull(item);

            var lockedItem = await _table.SearchSingleAndLockWhereAsync("Id=@Id", new { Id = item.Id });
            Assert.IsNotNull(lockedItem);

        }

        [TestMethod]
        public async Task ClaimLock_Given_ItemNotExists_Gives_Null()
        {
            var lockedItem = await _table.SearchSingleAndLockWhereAsync("Id=@Id", new { Id = Guid.NewGuid() });
            Assert.IsNull(lockedItem);
        }

        [TestMethod]
        public async Task ClaimLock_Given_Scope_Gives_ReturnItem()
        {
            var createItem = new TestItemBare
            {
                Value = "x"
            };
            var item = await _table.CreateAndReturnAsync(createItem);
            Assert.IsNotNull(item);

            bool? lockSuccess = null;

            using (var scope = CreateStandardScope())
            {
                try
                {
                    var lockedItem = await _table.SearchSingleAndLockWhereAsync("Id=@Id", new { Id = item.Id });
                    FulcrumAssert.IsNotNull(lockedItem);
                    lockSuccess = true;
                    scope.Complete();
                }
                catch (Exception)
                {
                    lockSuccess = false;
                }
            }
            Assert.IsNotNull(lockSuccess);
            Assert.IsTrue(lockSuccess);
        }

        [TestMethod]
        public async Task ClaimLock_Given_AlreadyLocked_Gives_FulcrumResourceLockedException()
        {
            var createItem = new TestItemBare
            {
                Value = "x"
            };
            var item = await _table.CreateAndReturnAsync(createItem);
            Assert.IsNotNull(item);
            bool? lock2Success = null;

            var claimLock1Success = new ManualResetEvent(false);
            var hasTriedToClaimLock2 = new ManualResetEvent(false);
            var thread1 = ThreadHelper.FireAndForget(async () =>
            {
                using (var scope = CreateStandardScope())
                {
                    var lockedItem = await _table.SearchSingleAndLockWhereAsync("Id=@Id", new { Id = item.Id });
                    FulcrumAssert.IsNotNull(lockedItem);
                    claimLock1Success.Set();
                    FulcrumAssert.IsTrue(hasTriedToClaimLock2.WaitOne(TimeSpan.FromSeconds(1)), CodeLocation.AsString());
                    scope.Complete();
                }
            });
            var thread2 = ThreadHelper.FireAndForget(async () =>
            {
                FulcrumAssert.IsTrue(claimLock1Success.WaitOne(TimeSpan.FromSeconds(1)));
                using (var scope = CreateStandardScope())
                {
                    try
                    {
                        var lockedItem = await _table.SearchSingleAndLockWhereAsync("Id=@Id", new { Id = item.Id });
                        FulcrumAssert.IsNotNull(lockedItem);
                        lock2Success = true;
                        scope.Complete();
                    }
                    catch (FulcrumResourceLockedException)
                    {
                        lock2Success = false;
                    }
                }
                hasTriedToClaimLock2.Set();
            });
            Assert.IsTrue(thread2.Join(TimeSpan.FromSeconds(5)));
            Assert.IsNotNull(lock2Success);
            Assert.IsFalse(lock2Success);
            Assert.IsTrue(thread1.Join(TimeSpan.FromSeconds(1)));
        }

        [TestMethod]
        public async Task ClaimLock_Given_ScopeCompleted_Gives_NewLock()
        {
            var createItem = new TestItemBare
            {
                Value = "x"
            };
            var item = await _table.CreateAndReturnAsync(createItem);
            Assert.IsNotNull(item);
            bool? lock2Success = null;

            var scopeCompleted = new ManualResetEvent(false);
            var hasTriedToClaimLock2 = new ManualResetEvent(false);
            var thread1 = ThreadHelper.FireAndForget(async () =>
            {
                using (var scope = CreateStandardScope())
                {
                    var lockedItem = await _table.SearchSingleAndLockWhereAsync("Id=@Id", new { Id = item.Id });
                    FulcrumAssert.IsNotNull(lockedItem);
                    scope.Complete();
                }
                scopeCompleted.Set();
            });
            var thread2 = ThreadHelper.FireAndForget(async () =>
            {
                FulcrumAssert.IsTrue(scopeCompleted.WaitOne(TimeSpan.FromSeconds(100)));
                using (var scope = CreateStandardScope())
                {
                    try
                    {
                        var lockedItem = await _table.SearchSingleAndLockWhereAsync("Id=@Id", new { Id = item.Id });
                        FulcrumAssert.IsNotNull(lockedItem);
                        lock2Success = true;
                        scope.Complete();
                    }
                    catch (FulcrumResourceLockedException)
                    {
                        lock2Success = false;
                    }
                }
            });
            Assert.IsTrue(thread2.Join(TimeSpan.FromSeconds(5)));
            Assert.IsNotNull(lock2Success);
            Assert.IsTrue(lock2Success);
            Assert.IsTrue(thread1.Join(TimeSpan.FromSeconds(1)));
        }

        /// <summary>
        /// Create a scope with our standard options
        /// </summary>
        /// <returns></returns>
        private static TransactionScope CreateStandardScope()
        {

            var options = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted
            };
            return new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}
#endif