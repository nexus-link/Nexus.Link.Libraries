namespace Nexus.Link.Libraries.Core.Storage.Logic.SequentialGuid;

public enum SequentialGuidType
{
    /// <summary>
    /// The first six bytes are sequential and this is preferred for MySQL and Raven DB.
    /// </summary>
    AsString,
    /// <summary>
    /// The first six bytes are sequential and this is preferred for Oracle.
    /// </summary>
    AsBinary,
    /// <summary>
    /// The last six bytes are sequential, which is what SQL Server uses.
    /// </summary>
    AtEnd
}