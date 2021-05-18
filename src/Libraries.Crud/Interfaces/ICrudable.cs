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
}
