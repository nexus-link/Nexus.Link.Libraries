namespace Nexus.Link.Libraries.SqlServer.Test
{
    public static class TestSettings
    {
        public static string ConnectionString = "Data Source=.;Initial Catalog=LibrariesSqlServerUnitTest;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        public static string AzureConnectionString = "Server=tcp:nonprd-dbserver.database.windows.net,1433;Initial Catalog=LibrariesSqlServerTests;Persist Security Info=False;User ID=nonprd-admin;Password=wXc3Yb26n9tv6bNxs;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
    }
}
