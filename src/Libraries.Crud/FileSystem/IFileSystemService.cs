using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Crud.Interfaces;

namespace Nexus.Link.Libraries.Crud.FileSystem;

/// <summary>
/// A service for dealing with <see cref="FileOrDirectory"/>.
/// </summary>
public interface IFileSystemService : 
    ICreateWithSpecifiedId<FileOrDirectory, string>,
    IRead<FileOrDirectory, string>,
    IReadChildren<FileOrDirectory, string>,
    IDeleteChildren<string>
{
    Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

    Task<TextWriter> CreateTextWriterAsync(string id);
    Task<string> ReadContentAsStringAsync(FileOrDirectory file);
}