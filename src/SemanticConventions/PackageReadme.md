# {{RootNamespace}}

The **stable** [OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/) as C# constants and metric instrument factories, generated with [OpenTelemetry Weaver](https://github.com/open-telemetry/weaver).

```csharp
using {{RootNamespace}};

activity?.SetTag(HttpAttributes.HttpRequestMethod, HttpAttributes.HttpRequestMethodValues.Get);
activity?.SetTag(HttpAttributes.HttpRequestHeader("content-type"), contentType);

// Metric name, unit and description, or a ready-made instrument
var duration = HttpMetrics.CreateServerRequestDuration(meter); // Histogram<double>
duration.Record(elapsed.TotalSeconds,
    new KeyValuePair<string, object?>(HttpAttributes.HttpRequestMethod, HttpAttributes.HttpRequestMethodValues.Get));
```

Counters, up-down counters and gauges also have an overload that creates the observable instrument from a callback.

For experimental conventions, use `{{RootNamespace}}.Incubating`.
