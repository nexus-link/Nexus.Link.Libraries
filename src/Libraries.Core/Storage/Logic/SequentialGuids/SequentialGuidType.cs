namespace Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuids;

public enum SequentialGuidType
{
    /// <summary>
    /// The first six bytes are sequential.
    /// </summary>
    /// <remarks>
    /// Use this for MySQL and Raven DB.
    /// </remarks>
    AsString,
    /// <summary>
    /// The first six bytes are sequential.
    /// </summary>
    /// <remarks>
    /// Use this for Oracle DB
    /// </remarks>
    AsBinary,
    /// <summary>
    /// The last six bytes are sequential.
    /// </summary>
    /// <remarks>
    /// Use this for Microsoft SQL Server
    /// </remarks>
    AtEnd
}