using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Libraries.Crud.FileSystem.Local;

public class LocalFileSystem: IFileSystemService
{
    private readonly IFileSystem _fileSystem;

    public LocalFileSystem(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public Task CreateWithSpecifiedIdAsync(string id, FileOrDirectory item,
        CancellationToken cancellationToken = default)
    {
        InternalContract.RequireNotNullOrWhiteSpace(id, nameof(id));
        InternalContract.RequireNotNull(item, nameof(item));
        InternalContract.RequireValidated(item, nameof(item));
        InternalContract.Require(!item.IsFile, $"Parameter {nameof(item)} must have {nameof(item.IsFile)} == false");

        _fileSystem.Directory.CreateDirectory(id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<FileOrDirectory> CreateWithSpecifiedIdAndReturnAsync(string id, FileOrDirectory item,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task DeleteChildrenAsync(string parentId, CancellationToken cancellationToken = default)
    {
        foreach (var fileOrDirectory in await ReadChildrenAsync(parentId, cancellationToken:cancellationToken))
        {
            if (fileOrDirectory.IsFile)
            {
                _fileSystem.File.Delete(fileOrDirectory.Id);
            }
            else
            {
                await DeleteChildrenAsync(fileOrDirectory.Id, cancellationToken);
                _fileSystem.Directory.Delete(fileOrDirectory.Id);
            }
        }
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return _fileSystem.Directory.Exists(path);
    }

    /// <inheritdoc />
    public async Task<TextWriter> CreateTextWriterAsync(string id)
    {
        await Task.CompletedTask;
        return _fileSystem.File.CreateText(id);
    }

    /// <inheritdoc />
    public Task<string> ReadContentAsStringAsync(FileOrDirectory file)
    {
        InternalContract.RequireNotNull(file, nameof(file));
        InternalContract.RequireValidated(file, nameof(file));
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FileOrDirectory>> ReadChildrenAsync(string parentId, int limit = 2147483647,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var list = new List<FileOrDirectory>();
        if (!_fileSystem.Directory.Exists(parentId)) return list;
        var filePaths = _fileSystem.Directory.EnumerateFiles(parentId);
        list.AddRange(filePaths.Select(p => new FileOrDirectory
        {
            Id = p,
            Name = "*unknown*",
            IsFile = true,
            ParentDirectoryId = parentId
        }));
        var directoryPaths = _fileSystem.Directory.EnumerateDirectories(parentId);
        list.AddRange(directoryPaths.Select(p => new FileOrDirectory
        {
            Id = p,
            Name = "*unknown*",
            IsFile = false,
            ParentDirectoryId = parentId
        }));
        await Task.CompletedTask;
        FulcrumAssert.IsValidated(list, CodeLocation.AsString());
        return list;
    }

    /// <inheritdoc />
    public Task<FileOrDirectory> ReadAsync(string id, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}