using Nexus.Link.Libraries.Core.EntityAttributes;
using Nexus.Link.Libraries.Core.Storage.Model;

namespace Nexus.Link.Libraries.Crud.FileSystem;

/// <summary>
/// FileOrDirectory information
/// </summary>
public class FileOrDirectory : IUniquelyIdentifiable<string>
{
    /// <summary>
    /// The primary key for the directory. For a regular file system this could be the path to the directory.
    /// </summary>
    [Validation.NotNullOrWhitespace]
    public string Id { get; set; } = null!;

    /// <summary>
    /// The name of the file or directory
    /// </summary>
    [Validation.NotNullOrWhitespace]
    public string Name { get; set; } = null;

    /// <summary>
    /// The primary key for the parent directory. This should be null for the root directory. 
    /// </summary>
    public string ParentDirectoryId { get; set; } = null!;

    /// <summary>
    /// True = file
    /// False = directory 
    /// </summary>
    public bool IsFile { get; set; } = false;

}