using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Primitives;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OTel.Jaeger.App.Model;
using OTel.Jaeger.App.Telemetry;
using System.Diagnostics;
using System.Text;

namespace OTel.Jaeger.App.Receiver
{
    public class EventHubReceiverService : IEventHubReceiverService
    {
        private static PartitionReceiver _receiver;

        private string _connectionString = "<< CONNECTION STRING FOR THE EVENT HUBS NAMESPACE >>";
        private string _eventHubName = "<< NAME OF THE EVENT HUB >>";
        private string _eventHubPartition = string.Empty;

        public EventHubReceiverService(IConfiguration configuration)
        {
            _connectionString = configuration["EventHubs:ConnectionString"];
            _eventHubName = configuration["EventHubs:Name"];
            _eventHubPartition = configuration["EventHubs:Partition"];
        }


        public async Task<OTelEventData> ReceiveLatestAsync()
        {
            int batchSize = 1;
            TimeSpan waitTime = TimeSpan.FromSeconds(1);
            var dataAsJson = string.Empty;
            OTelEventData oTelEventData = new OTelEventData();
            string consumerGroup = string.Empty;
            IEnumerable<EventData> eventBatch = new List<EventData>();

            EventHubConsumerClient consumer;

            using (var createClientAct = OtelTelemetry.OtelTelemetrySource.StartActivity("Create EventHub Consumer Client"))
            {
                consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
                consumer = new EventHubConsumerClient(consumerGroup, _connectionString, _eventHubName);
            }

            using var cancellationSource = new CancellationTokenSource();

            var partitionProps = await consumer.GetPartitionPropertiesAsync(_eventHubPartition);
            if (partitionProps.IsEmpty == true)
            {
                return new OTelEventData();
            }

            var startingPosition = EventPosition.Latest;

            var receiverOptions = new PartitionReceiverOptions() { OwnerLevel = 0 }; // This is to forcibly own the connection

            using (var receiveClientAct = OtelTelemetry.OtelTelemetrySource.StartActivity("Receive with EventHub Consumer Client"))
            {
                _receiver = new PartitionReceiver(consumerGroup, _eventHubPartition, startingPosition, _connectionString, _eventHubName, receiverOptions);

                eventBatch = await _receiver.ReceiveBatchAsync(batchSize, waitTime, cancellationSource.Token);
            }

            foreach (EventData eventData in eventBatch)
            {
                if (eventData.Data == null) break;

                dataAsJson = Encoding.UTF8.GetString(eventData.Data);

                oTelEventData = JsonConvert.DeserializeObject<OTelEventData>(dataAsJson);
                return oTelEventData;
            }

            return new OTelEventData();
        }

    }
}
