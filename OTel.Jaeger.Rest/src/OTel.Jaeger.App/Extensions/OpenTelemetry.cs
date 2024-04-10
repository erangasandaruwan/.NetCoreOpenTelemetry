using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OTel.Jaeger.App.Extensions
{
    public static class OtelOpenTelemetry
    {
        public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOpenTelemetry()
                   .WithTracing(b =>
                   {
                       b.AddSource("OTel.Jaeger.Rest")
                         .ConfigureResource(resource => resource.AddService(serviceName: "OTel.Jaeger.Rest", serviceVersion: "1.0.0.0"))
                         .AddAspNetCoreInstrumentation()
                         .AddConsoleExporter()
                         .AddOtlpExporter(e => { e.Endpoint = new Uri("http://localhost:4317"); });
                   });

            return services;
        }
    }
}
