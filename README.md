# .NetCoreOpenTelemetry

**OpenTelemetry** is a framework where is has the the ability to understand the internal state of a system by examining its outputs and a toolkit designed to create and manage telemetry data such as traces, metrics, and logs. This can be used with a broad variety of Observability backends, including open source tools like Jaeger, Zipkin and Prometheus, as well as commercial offerings.

OpenTelemetry is not an observability backend or telemetry collector like **Jaeger**, **Zipkin**, **Prometheus**, or other commercial vendors. OpenTelemetry basically do the generation, collection, management, and export of telemetry data to any configured obervability backend. A major goal of OpenTelemetry is that we can easily instrument your applications or systems, no matter their language, infrastructure, or runtime environment. Crucially, the storage and visualization of telemetry is intentionally left to other tools. With the rise of distributed computing, microservices & event driven architectures, and increasingly complex business requirements, the need for software and infrastructure observability which is the ability to understand the internal state of a system by examining its outputs is becoming more crutial than ever.

### Configure OpenTelemetry with .Net

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
