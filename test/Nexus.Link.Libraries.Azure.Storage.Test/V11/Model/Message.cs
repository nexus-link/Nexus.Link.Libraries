﻿using Microsoft.WindowsAzure.Storage.Queue;
using Nexus.Link.Libraries.Azure.Storage.Queue;

namespace Nexus.Link.Libraries.Azure.Storage.Test.V11.Model
{
    public class Message : IAzureStorageQueueMessage
    {
        public string Name { get; set; }
        public CloudQueueMessage QueueMessage { get; set; }
    }
}
