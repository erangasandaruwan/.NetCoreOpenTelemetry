using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Configuration;
using OTel.Jaeger.App.Constants;
using OTel.Jaeger.App.Sender;

namespace OTel.Jaeger.App.Extensions
{
    public static class EventHubClient
    {
        public static void ConfigureClients(IConfiguration configuration)
        {
            EventHubProducerClientOptions eventHubClientOptions = new EventHubProducerClientOptions
            {
                RetryOptions = new EventHubsRetryOptions
                {
                    TryTimeout = TimeSpan.FromSeconds(EventHubConstants.EventHubTimeOut),
                    MaximumRetries = EventHubConstants.EventHubMaximumRetries,
                    Mode = EventHubsRetryMode.Fixed
                }
            };

            var eventHubName = configuration["EventHubs:Name"];
            var eventHubConnectionString = configuration["EventHubs:ConnectionString"];

            EventHubSenderService.InitEventHubProducerClient(eventHubName, eventHubConnectionString, eventHubClientOptions);
        }
    }
}
