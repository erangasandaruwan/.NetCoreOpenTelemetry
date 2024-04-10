using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace OTel.Jaeger.App.Sender
{
    public class EventHubSenderService : IEventHubSenderService
    {
        private string _eventHubPartition = string.Empty;

        private static EventHubProducerClient _eventHubProducerClient;

        public static void InitEventHubProducerClient(string eventHubName, string eventHubConnectionString, EventHubProducerClientOptions eventHubClientOptions)
        {
            if (_eventHubProducerClient == null || _eventHubProducerClient.IsClosed)
                _eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName, eventHubClientOptions);
        }

        public EventHubSenderService(IConfiguration configuration)
        {
            _eventHubPartition = configuration["EventHubs:Partition"];
        }

        public async Task<bool> SendDataToPartitionAsync<T>(T data)
        {
            var batchOptions = new CreateBatchOptions { PartitionId = _eventHubPartition };
            var eventDataBatch = await _eventHubProducerClient.CreateBatchAsync(batchOptions);

            var dataAsJson = JsonConvert.SerializeObject(data);
            eventDataBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(dataAsJson)));

            await _eventHubProducerClient.SendAsync(eventDataBatch);
            return true;
        }
    }
}
