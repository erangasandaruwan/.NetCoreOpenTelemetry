using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTel.Jaeger.App.Telemetry
{
    public static class OtelTelemetry
    {
        public static readonly ActivitySource OtelTelemetrySource = new("SampleService");
    }
}
