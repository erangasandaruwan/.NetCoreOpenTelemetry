# OpenTelemetry with .NetCore

**OpenTelemetry** is a framework where is has the the ability to understand the internal state of a system by examining its outputs and a toolkit designed to create and manage telemetry data such as traces, metrics, and logs. This can be used with a broad variety of Observability backends, including open source tools like Jaeger, Zipkin and Prometheus, as well as commercial offerings.

OpenTelemetry is not an observability backend or telemetry collector like **Jaeger**, **Zipkin**, **Prometheus**, or other commercial vendors. OpenTelemetry basically do the generation, collection, management, and export of telemetry data to any configured obervability backend. A major goal of OpenTelemetry is that we can easily instrument your applications or systems, no matter their language, infrastructure, or runtime environment. Crucially, the storage and visualization of telemetry is intentionally left to other tools. With the rise of distributed computing, microservices & event driven architectures, and increasingly complex business requirements, the need for software and infrastructure observability which is the ability to understand the internal state of a system by examining its outputs is becoming more crutial than ever.

### Configure OpenTelemetry with .Net

It is possible to configure and start tracing with either Console applications as well as ASP.NET Core-based applications. Here we can see how we can do the needful to setup and start tracing with .Net Core Web API solutions. The target .Net Core version is 8.0.

For this, it requires to install the below nuget packages. Here its with Package Manager.
```html
Install-Package OpenTelemetry
Install-Package OpenTelemetry.Extensions.Hosting
Install-Package OpenTelemetry.Exporter.Console
```


```csharp

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
                   .WithTracing(b =>
                   {
                       b.AddSource("SampleService")
                           .ConfigureResource(resource =>
                              resource.AddService(serviceName: "SampleService",
                                                  serviceVersion: "1.0.0.0"))
                           .AddAspNetCoreInstrumentation()
                           .AddConsoleExporter();
                   });
```

### Setting up an ActivitySource

Once tracing is configured and initialized, it is possible to configure an ActivitySource, which will be how it traces operations with Activity elements.
An **ActivitySource** is instantiated once per application/service that is being instrumented, so it’s a good idea to instantiate it once in a shared location. It is also typically named the same as the Service Name (Here it has used **SampleService**).

| :memo:        | It is recomonded to instantiate ActivitySource as an static reference   |
|---------------|:---------------------------------------------|

```csharp
using System.Diagnostics;

public static class OtelTelemetry
{
    public static readonly ActivitySource OtelTelemetrySource = new("SampleService");
}
```

### Creating Activities and Nested Activities

It is possible to give any name or definitions and start activities. If it needs to track any sub-operation, it is also possible to have the traces in hierarchical way else possible to create independent traces as well. 

> [!NOTE]  
> Here it has provided a sample code which is used to connect and read data from event hub.

```csharp
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
    _receiver = new PartitionReceiver(consumerGroup, _eventHubPartition, startingPosition,
                                            _connectionString, _eventHubName, receiverOptions);

    eventBatch = await _receiver.ReceiveBatchAsync(batchSize, waitTime, cancellationSource.Token);
}
```

### Get the current Activity & Add tags to an Activity

It is possible to access current Activity and possible to enrich it with more information.

```csharp
var activity = Activity.Current;
// may be null if there is none

activity?.SetTag("operation.value", 1);
activity?.SetTag("operation.name", "Receive");
activity?.SetTag("operation.received-batch", eventBatch);
```
