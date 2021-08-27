namespace Nexus.Link.Libraries.Crud.Interfaces
{
    /// <summary>
    /// Indicates that the implementor has one or more crud methods./>.
    /// </summary>
    public interface ICrudable
    {
    }

    /// <inheritdoc />
    public interface ICrudable<in TId> : ICrudable
    {
    }

    /// <inheritdoc />
    public interface ICrudable<in TModel, in TId> : ICrudable<TId>
    {
    }

    /// <inheritdoc />
    public interface ICrudable<in TModelCreate, in TModel, in TId> : ICrudable<TModel, TId>
        where TModel : TModelCreate
    {
    }

    /// <inheritdoc />
    public interface ICrudableDependent<in TId, in TDependentId> : ICrudable<TId>
    {
    }

    /// <inheritdoc />
    public interface ICrudableDependent<in TModel, in TId, in TDependentId> : ICrudableDependent<TId, TDependentId>
    {
    }

    /// <inheritdoc />
    public interface ICrudableDependent<in TModelCreate, in TModel, in TId, in TDependentId> : ICrudableDependent<TModel, TId, TDependentId>
        where TModel : TModelCreate
    {
    }
}
