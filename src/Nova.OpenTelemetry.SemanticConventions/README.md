# Nova.OpenTelemetry.SemanticConventions

The **stable** [OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/) as C# constants, generated with [OpenTelemetry Weaver](https://github.com/open-telemetry/weaver).

```csharp
using Nova.OpenTelemetry.SemanticConventions;

activity?.SetTag(HttpAttributes.HttpRequestMethod, HttpAttributes.HttpRequestMethodValues.Get);
activity?.SetTag(HttpAttributes.HttpRequestHeader("content-type"), contentType);
```

For experimental conventions, use `Nova.OpenTelemetry.SemanticConventions.Incubating`.
