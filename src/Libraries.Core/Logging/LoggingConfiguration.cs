
namespace Nexus.Link.Libraries.Core.Logging
{
    public class LoggingConfiguration
    {
        public string ConnectionString { get; }
        public string QueueName { get; }
        public string Version { get;  }

        public LoggingConfiguration(string storageConnectionString, string queueName, string version)
        {
            ConnectionString = storageConnectionString;
            QueueName = queueName;
            Version = version;
        }
    }
}