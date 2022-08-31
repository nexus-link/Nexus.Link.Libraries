using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Model;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit.Persistence.Services;
using Nexus.Link.Libraries.Crud.Web.Test.FileSystemTests.Support;
using Nexus.Link.Libraries.Web.RestClientHelper;
using Shouldly;

namespace Nexus.Link.Libraries.Crud.Web.Test.FileSystemTests;

[TestClass]
public class PersistenceTests : IntegrationTestsBase
{
    private IHttpSender _projectHttpSender = null!;
    private IHttpSender _repositoryHttpSender = null!;

    [Ignore] [TestMethod]
    public async Task T01_GetProject()
    {
        // Arrange
        var iut = new ProjectService(OrganizationApiSender);

        // Act
        var projects = (await iut.ReadAllAsync()).ToArray();

        // Assert
        projects.Any().ShouldBe(true);
        var project = projects.FirstOrDefault(p => p.Name == "Pedigree");
        project.ShouldNotBeNull();
    }

    [Ignore] [TestMethod]
    public async Task T02_GetRepository()
    {
        // Arrange
        var project = await GetProject();
        FulcrumAssert.IsNotNull(project, CodeLocation.AsString());
        _projectHttpSender = OrganizationApiSender.CreateHttpSender(project.Id);
        var iut = new RepositoryService(OrganizationApiSender, project.Id);

        // Act
        var repositories = (await iut.ReadAllAsync()).ToArray();

        // Assert
        var repository = repositories.FirstOrDefault(p => p.Name == "model");
        repository.ShouldNotBeNull();
    }

    [Ignore] [TestMethod]
    public async Task T03_GetBranch()
    {
        // Arrange
        var repository = await GetRepository();
        FulcrumAssert.IsNotNull(repository, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_projectHttpSender, CodeLocation.AsString());
        var iut = new BranchService(_projectHttpSender, repository.Id);
        _repositoryHttpSender = OrganizationApiSender.CreateHttpSender($"_apis/git/repositories/{repository.Id}");

        // Act
        var branches = (await iut.ReadAllAsync()).ToArray();

        // Assert
        var branch = branches.FirstOrDefault(p => p.Name == "refs/heads/main");
        branch.ShouldNotBeNull();
    }

    [Ignore] [TestMethod]
    public async Task T04_GetTreeItem()
    {
        // Arrange
        var branch = await GetBranch();
        FulcrumAssert.IsNotNull(branch, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new ItemService(_repositoryHttpSender);

        // Act
        var treeItems = (await iut.ReadAllAsync()).ToArray();

        // Assert
        var treeItem = treeItems.FirstOrDefault(p => p.Name == "/");
        treeItem.ShouldNotBeNull();
    }

    [Ignore] [TestMethod]
    public async Task TF05_GetRootTreeTree()
    {
        // Arrange
        var treeItem = await GetTreeItem();
        FulcrumAssert.IsNotNull(treeItem, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new TreeService(_repositoryHttpSender);

        // Act
        var tree = await iut.ReadAsync(treeItem.Id);

        // Assert
        var subTree = tree.TreeEntries.FirstOrDefault(p => p.GitObjectType == GitObjectTypeEnum.Tree && p.Name == "src");
        subTree.ShouldNotBeNull();
    }

    [Ignore] [TestMethod]
    public async Task T06_GetRootTreeBlob()
    {
        // Arrange
        var treeItem = await GetTreeItem();
        FulcrumAssert.IsNotNull(treeItem, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new TreeService(_repositoryHttpSender);

        // Act
        var tree = await iut.ReadAsync(treeItem.Id);

        // Assert
        var blob = tree.TreeEntries.FirstOrDefault(p => p.GitObjectType == GitObjectTypeEnum.Blob && p.Name == ".gitignore");
        blob.ShouldNotBeNull();
    }

    [Ignore] [TestMethod]
    public async Task T07_GetTreeTree()
    {
        // Arrange
        var parentTree = await GetRootTreeTree();
        FulcrumAssert.IsNotNull(parentTree, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new TreeService(_repositoryHttpSender);

        // Act
        var tree = await iut.ReadAsync(parentTree.Id);

        // Assert
        var subTree = tree.TreeEntries.FirstOrDefault(p => p.GitObjectType == GitObjectTypeEnum.Tree && p.Name == "nugets");
        subTree.ShouldNotBeNull();
    }
    private async Task<Resource> GetProject()
    {
        // Arrange
        var iut = new ProjectService(OrganizationApiSender);

        // Act
        var projects = (await iut.ReadAllAsync()).ToArray();

        // Assert
        projects.Any().ShouldBe(true);
        var project = projects.FirstOrDefault(p => p.Name == "Pedigree");
        project.ShouldNotBeNull(); ;
        return project;
    }

    [Ignore] [TestMethod]
    private async Task<Resource> GetRepository()
    {
        // Arrange
        var project = await GetProject();
        FulcrumAssert.IsNotNull(project, CodeLocation.AsString());
        _projectHttpSender = OrganizationApiSender.CreateHttpSender(project.Id);
        var iut = new RepositoryService(OrganizationApiSender, project.Id);

        // Act
        var repositories = (await iut.ReadAllAsync()).ToArray();

        // Assert
        var repository = repositories.FirstOrDefault(p => p.Name == "model");
        repository.ShouldNotBeNull();
        return repository;
    }

    [Ignore] [TestMethod]
    private async Task<Resource> GetBranch()
    {
        // Arrange
        var repository = await GetRepository();
        FulcrumAssert.IsNotNull(repository, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_projectHttpSender, CodeLocation.AsString());
        var iut = new BranchService(_projectHttpSender, repository.Id);
        _repositoryHttpSender = OrganizationApiSender.CreateHttpSender($"_apis/git/repositories/{repository.Id}");

        // Act
        var branches = (await iut.ReadAllAsync()).ToArray();

        // Assert
        var branch = branches.FirstOrDefault(p => p.Name == "refs/heads/main");
        branch.ShouldNotBeNull();
        return branch;
    }

    [Ignore] [TestMethod]
    private async Task<Resource> GetTreeItem()
    {
        // Arrange
        var branch = await GetBranch();
        FulcrumAssert.IsNotNull(branch, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new ItemService(_repositoryHttpSender);

        // Act
        var treeItems = (await iut.ReadAllAsync()).ToArray();

        // Assert
        var treeItem = treeItems.FirstOrDefault(p => p.Name == "/");
        treeItem.ShouldNotBeNull();
        return treeItem;
    }

    [Ignore] [TestMethod]
    private async Task<Resource> GetRootTreeTree()
    {
        // Arrange
        var treeItem = await GetTreeItem();
        FulcrumAssert.IsNotNull(treeItem, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new TreeService(_repositoryHttpSender);

        // Act
        var tree = await iut.ReadAsync(treeItem.Id);

        // Assert
        var subTree = tree.TreeEntries.FirstOrDefault(p => p.GitObjectType == GitObjectTypeEnum.Tree && p.Name == "src");
        subTree.ShouldNotBeNull();
        return subTree;
    }

    [Ignore] [TestMethod]
    private async Task<Resource> GetRootTreeBlob()
    {
        // Arrange
        var treeItem = await GetTreeItem();
        FulcrumAssert.IsNotNull(treeItem, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new TreeService(_repositoryHttpSender);

        // Act
        var tree = await iut.ReadAsync(treeItem.Id);

        // Assert
        var blob = tree.TreeEntries.FirstOrDefault(p => p.GitObjectType == GitObjectTypeEnum.Blob && p.Name == ".gitignore");
        blob.ShouldNotBeNull();
        return blob;
    }

    [Ignore] [TestMethod]
    private async Task<Resource> GetTreeTree()
    {
        // Arrange
        var parentTree = await GetRootTreeTree();
        FulcrumAssert.IsNotNull(parentTree, CodeLocation.AsString());
        FulcrumAssert.IsNotNull(_repositoryHttpSender, CodeLocation.AsString());
        var iut = new TreeService(_repositoryHttpSender);

        // Act
        var tree = await iut.ReadAsync(parentTree.Id);

        // Assert
        var subTree = tree.TreeEntries.FirstOrDefault(p => p.GitObjectType == GitObjectTypeEnum.Tree && p.Name == "nugets");
        subTree.ShouldNotBeNull();
        return subTree;
    }
}