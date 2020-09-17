using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Nexus.Link.Libraries.Crud.UnitTests.ManyToOne;
using Nexus.Link.Libraries.Crud.UnitTests.Model;

namespace Nexus.Link.Libraries.Crud.Test.Storage
{
    [TestClass]
    public class MemoryManyToOneTest : TestIManyToOne<Guid, Guid?>
    {
        private ICrud<TestItemId<Guid>, Guid> _oneStorage;
        private ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid> _crudManyStorage;

        [TestInitialize]
        public void Inititalize()
        {
            _oneStorage = new CrudMemory<TestItemId<Guid>, Guid>();
            _crudManyStorage = new ManyToOneMemory<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid>(item => item.ParentId);
        }

        /// <inheritdoc />
        protected override ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid>
            CrudManyStorageRecursive => null;

        /// <inheritdoc />
        protected override ICrudManyToOne<TestItemManyToOneCreate<Guid?>, TestItemManyToOne<Guid, Guid?>, Guid>
            CrudManyStorageNonRecursive => _crudManyStorage;

        /// <inheritdoc />
        protected override ICrud<TestItemId<Guid>, Guid> OneStorage => _oneStorage;
    }
}
