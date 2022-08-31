using Microsoft.Rest;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Web.RestClientHelper;

namespace Nexus.Link.Libraries.Crud.Web.Test.FileSystemTests.Support;

public class IntegrationTestsBase
{
    protected readonly string _organizationUrl;

    protected HttpSender OrganizationApiSender { get; }
    //private readonly AzureDevopsFileSystemCapability _fileSystemCapability;

    public IntegrationTestsBase()
    {
        FulcrumApplicationHelper.UnitTestSetup(nameof(PersistenceTests));
        var organization = "nexuslink";
        _organizationUrl = @$"https://dev.azure.com/{organization}";
        // https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows#create-a-pat
        var credentials = new BasicAuthenticationCredentials
        {
            UserName = "test-git-read",
            Password = ""
        };
        OrganizationApiSender = new HttpSender(_organizationUrl, credentials);
    }
}