using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.FileSystem;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit;

public class AzureDevopsFileSystem : IFileSystemService
{
    private readonly TreeService _treeService;

    public AzureDevopsFileSystem(IHttpSender httpSender)
    {
        _treeService = new TreeService(httpSender);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var x = await ReadAsync(path, cancellationToken);
        return x != null;
    }

    /// <inheritdoc />
    public Task<TextWriter> CreateTextWriterAsync(string id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<string> ReadContentAsStringAsync(FileOrDirectory file)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task CreateWithSpecifiedIdAsync(string id, FileOrDirectory item,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<FileOrDirectory> CreateWithSpecifiedIdAndReturnAsync(string id, FileOrDirectory item,
        CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<FileOrDirectory>> ReadChildrenAsync(string parentId, int limit = 2147483647,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var tree = await _treeService.ReadAsync(parentId, cancellationToken);
        var result = tree.TreeEntries.Select(e =>
            new FileOrDirectory
            {
                Id = e.Id,
                Name = e.Name,
                IsFile = e.GitObjectType == GitObjectTypeEnum.Blob,
                ParentDirectoryId = parentId
            }).ToList();
        FulcrumAssert.IsValidated(result, CodeLocation.AsString());
        return result;
    }

    /// <inheritdoc />
    public Task DeleteChildrenAsync(string parentId, CancellationToken cancellationToken = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<FileOrDirectory> ReadAsync(string id, CancellationToken cancellationToken = new CancellationToken())
    {
        var tree = await ReadChildrenAsync(id, 1, cancellationToken);
        var fileOrDirectory = new FileOrDirectory
        {
            Id = id,
            Name = "*unknown*",
            IsFile = false,
            ParentDirectoryId = null
        };
        return fileOrDirectory;
    }
}