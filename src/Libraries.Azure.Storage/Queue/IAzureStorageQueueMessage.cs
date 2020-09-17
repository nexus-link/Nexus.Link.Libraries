using Microsoft.WindowsAzure.Storage.Queue;

namespace Nexus.Link.Libraries.Azure.Storage.Queue
{
    public interface IAzureStorageQueueMessage
    {
        CloudQueueMessage QueueMessage { get; set; }
    }
}