using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.Web.FileSystem.AzureDevopsGit;
using Nexus.Link.Libraries.Crud.Web.Test.FileSystemTests.Support;
using Shouldly;

namespace Nexus.Link.Libraries.Crud.Web.Test.FileSystemTests;

[TestClass]
public class AzureDevopsFileSystemTests : IntegrationTestsBase
{
    private AzureDevopsFileSystem _iut;

    [TestInitialize]
    public void Initialize()
    {
        _iut = new AzureDevopsFileSystem(OrganizationApiSender.CreateHttpSender("_apis/git/repositories/53d2dbc5-a7c8-40ae-87ae-da33c8cc2922"));
    }

    [Ignore] [TestMethod]
    public async Task T01_GetDirectoryContent()
    {
        // Arrange

        // Act
        var items = await _iut.ReadChildrenAsync("0fa662e6b55920db9e2b2b40ec934cb73bd1d087");

        // Assert
        items.Any().ShouldBe(true);
    }

    [Ignore] [TestMethod]
    public async Task T02_GetFile()
    {
        // Arrange
        var parentId = "0fa662e6b55920db9e2b2b40ec934cb73bd1d087";
        var entries = await _iut.ReadChildrenAsync(parentId);
        var entry = entries.FirstOrDefault(i => i.IsFile);
        FulcrumAssert.IsNotNull(entry, CodeLocation.AsString());

        // Act
        var file = await _iut.ReadAsync(entry!.Id);

        // Assert
        file.ShouldNotBeNull();
        file.Id.ShouldBe(entry.Id);
    }
}