# Nova.OpenTelemetry.SemanticConventions.Incubating

**All** [OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/) as C# constants, including experimental and deprecated ones, generated with [OpenTelemetry Weaver](https://github.com/open-telemetry/weaver).

Anything in this package that isn't in `Nova.OpenTelemetry.SemanticConventions` is not yet stable and may change or be removed in a later release.

- Stable definitions are marked `[Obsolete]` and point to their home in `Nova.OpenTelemetry.SemanticConventions`, so your code keeps compiling when a convention stabilises.
- Deprecated definitions are marked `[Obsolete]` with the reason from the registry.

```csharp
using Nova.OpenTelemetry.SemanticConventions.Incubating;

activity?.SetTag(GenAiIncubatingAttributes.GenAiRequestModel, "gpt-4o");
```
