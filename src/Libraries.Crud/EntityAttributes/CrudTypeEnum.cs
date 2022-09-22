namespace Nexus.Link.Libraries.Crud.EntityAttributes;


/// <summary>
/// If the function is a CRUD function, this this denotes which of them
/// </summary>
public enum CrudTypeEnum
{
    /// <summary>
    /// Not a CRUD function
    /// </summary>
    None,

    /// <summary>
    /// Create
    /// </summary>
    CreateOne,

    /// <summary>
    /// Read
    /// </summary>
    ReadOne,

    /// <summary>
    /// Read many
    /// </summary>
    ReadMany,

    /// <summary>
    /// Find one
    /// </summary>
    FindOne,

    /// <summary>
    /// Find many
    /// </summary>
    FindMany,

    /// <summary>
    /// Update
    /// </summary>
    UpdateOne,

    /// <summary>
    /// Delete
    /// </summary>
    DeleteOne,

    /// <summary>
    /// Delete many
    /// </summary>
    DeleteMany
}