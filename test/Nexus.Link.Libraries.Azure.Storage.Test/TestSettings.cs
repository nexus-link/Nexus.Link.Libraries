namespace Nexus.Link.Libraries.Azure.Storage.Test
{
    public static class TestSettings
    {
        public static string ConnectionString { get; } =
            "DefaultEndpointsProtocol=https;AccountName=libraries2azureunittest;AccountKey=EK3akYnMv/3JXQ2G8XQxhlNtVLzMFT/eKNtUF+BHBpUm9IZVbM4K6+Jbh7qhaS9k+dI3POgBuH/wfcLxYaqBKw==;EndpointSuffix=core.windows.net";
        public static string ConnectionStringNonExisting { get; } =
            "DefaultEndpointsProtocol=https;AccountName=libraries2azureunittestnonexist;AccountKey=EK3akYnMv/3JXQ2G8XQxhlNtVLzMFT/eKNtUF+BHBpUm9IZVbM4K6+Jbh7qhaS9k+dI3POgBuH/wfcLxYaqBKw==;EndpointSuffix=core.windows.net";
    }
}