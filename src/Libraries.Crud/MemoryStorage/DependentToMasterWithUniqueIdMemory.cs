using Nexus.Link.Libraries.Crud.Interfaces;
using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.Crud.Helpers;

namespace Nexus.Link.Libraries.Crud.MemoryStorage
{
    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class DependentToMasterWithUniqueIdMemory<TModel, TId, TDependentId> :
        DependentToMasterWithUniqueIdMemory<TModel, TModel, TId, TDependentId>,
        ICrudDependentToMaster<TModel, TId, TDependentId>
        where TModel : IUniquelyIdentifiableDependent<TId, TDependentId>, IUniquelyIdentifiable<TId>
    {
        /// <inheritdoc />
        public DependentToMasterWithUniqueIdMemory(ICrudManyToOne<TModel, TId> uniqueIdTable) : base(uniqueIdTable)
        {
        }
    }

    /// <summary>
    /// Functionality for persisting objects in groups.
    /// </summary>
    public class DependentToMasterWithUniqueIdMemory<TModelCreate, TModel, TId, TDependentId> :
        DependentToMasterWithUniqueIdConvenience<TModelCreate, TModel, TId, TDependentId>,
        ICrudDependentToMasterWithUniqueId<TModelCreate, TModel, TId, TDependentId>
        where TModel : TModelCreate, IUniquelyIdentifiableDependent<TId, TDependentId>
        where TModelCreate : IUniquelyIdentifiable<TId>
    {
        private readonly ICrudManyToOne<TModelCreate, TModel, TId> _uniqueIdTable;

        private readonly DependentToMasterConvenience<TModelCreate, TModel, TId, TDependentId> _convenience;

        public DependentToMasterWithUniqueIdMemory(ICrudManyToOne<TModelCreate, TModel, TId> uniqueIdTable)
        :base(uniqueIdTable)
        {
            _uniqueIdTable = uniqueIdTable;
            _convenience = new DependentToMasterConvenience<TModelCreate, TModel, TId, TDependentId>(this);
        }
    }
}