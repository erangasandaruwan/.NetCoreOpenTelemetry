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
                       b.AddSource("OTel.Jaeger.Rest")
                           .ConfigureResource(resource =>
                              resource.AddService(serviceName: "OTel.Jaeger.Rest",
                                                  serviceVersion: "1.0.0.0"))
                           .AddAspNetCoreInstrumentation()
                           .AddConsoleExporter();
                   });
```

### Setting up an ActivitySource

Once tracing is configured and initialized, it is possible to configure an ActivitySource, which will be how it traces operations with Activity elements.
An **ActivitySource** is instantiated once per application/service that is being instrumented, so it’s a good idea to instantiate it once in a shared location. It is also typically named the same as the Service Name (Here it has used **OTel.Jaeger.Rest**).

| :memo:        | It is recomonded to instantiate ActivitySource as an static reference   |
|---------------|:---------------------------------------------|

```csharp
using System.Diagnostics;

public static class OtelTelemetry
{
    public static readonly ActivitySource OtelTelemetrySource = new("OTel.Jaeger.Rest");
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

## Export Telemetry Data and Distributed Tracing

OpenTelemetry focuses on collecting data at the application level where as distributed tracing provides more complete and comprehensive visibility into the inner workings of the whole system including multiple software components, middlewares or inter-connected services and allows data to be broken down and sifted for useful information at a much more granular level.

## Use Jaeger to collect telemetry

As per the Jaeger documentation, it is a distributed tracing platform released as open source by [Uber Technologies](https://uber.github.io/#/)  . With Jaeger it is possible to,
- Monitor and troubleshoot distributed workflows
- Identify performance bottlenecks
- Track down root causes
- Analyze service dependencies


Let's try to install jaeger locally using docker desktop. To set up Jaeger as a service on Windows, ensure you have Docker installed on your Windows machine as Jaeger will be running in a Docker container for ease of use and management.

```docker
docker run --name jaeger -p 13133:13133 -p 16686:16686 -p 4317:4317 -d --restart=unless-stopped jaegertracing/opentelemetry-all-in-one
```

<img src="https://github.com/erangasandaruwan/.NetCoreOpenTelemetry/assets/25504137/1c65cbd0-1fdc-49af-917d-5592d014e0ba"  width="80%" height="40%">

> [!NOTE]  
> It is required to allocate at least 4GB of memory to Docker. Jaeger components, especially when running the full stack, can be memory-intensive. Further, that the necessary ports must kept open. Jaeger by default uses ports such as **6831/udp** for agent communication, **16686** for the web UI, and **14268** for collectors. Further 4317 can be used to send the telemetry data.

<img src="https://github.com/erangasandaruwan/.NetCoreOpenTelemetry/assets/25504137/a5c49e1b-d48d-4c46-8a6c-d2ad1c102baa"  width="100%" height="50%">


Configure .Net Core application to export OpenTelemetry to jeagger.

```csharp
services.AddOpenTelemetry()
       .WithTracing(b =>
       {
           b.AddSource("OTel.Jaeger.Rest")
             .ConfigureResource(resource => resource.AddService(serviceName: "OTel.Jaeger.Rest", serviceVersion: "1.0.0.0"))
             .AddAspNetCoreInstrumentation()
             .AddConsoleExporter()
             .AddOtlpExporter(e => { e.Endpoint = new Uri("http://localhost:4317"); });
       });
```


After invoking an API with the activities defined as in previous example, the traces will appear like this in the jaeger dashboard.

<img src="https://github.com/erangasandaruwan/.NetCoreOpenTelemetry/assets/25504137/890cc603-87a8-44a0-b453-d8a2af6fdeee"  width="100%" height="40%">

<img src="https://github.com/erangasandaruwan/.NetCoreOpenTelemetry/assets/25504137/b465c0db-e885-44ca-879b-042495f1ae4e"  width="100%" height="50%">

<img src="https://github.com/erangasandaruwan/.NetCoreOpenTelemetry/assets/25504137/f3b23ee6-2637-42c8-9df2-ada2e5c487f0"  width="100%" height="40%">

## Use Zipkin to collect telemetry

Zipkin is a distributed tracing system. It helps gather timing data needed to troubleshoot latency problems in service architectures. Features include both the collection and lookup of this data.

Let's try to install jaeger locally using docker desktop. 

```docker
docker run --rm -d -p 9411:9411 --name zipkin openzipkin/zipkin
```

<img src="https://github.com/erangasandaruwan/.NetCoreOpenTelemetry/assets/25504137/0fd34e11-88b3-4ea6-aa28-a6cc88ddf266"  width="80%" height="40%">

And here we go.
![image](https://github.com/erangasandaruwan/.NetCoreOpenTelemetry/assets/25504137/00d436b8-91d4-41ec-94ee-011234cc3b3d)



References 
- https://opentelemetry.io/docs/languages/net/instrumentation/
- https://www.jaegertracing.io/
- https://www.uber.com/en-SG/blog/distributed-tracing/
- https://zipkin.io/
