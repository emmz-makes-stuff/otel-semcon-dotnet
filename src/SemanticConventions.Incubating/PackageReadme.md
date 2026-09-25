# {{RootNamespace}}.Incubating

**All** [OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/) as C# constants and metric instrument factories, including experimental and deprecated ones, generated with [OpenTelemetry Weaver](https://github.com/open-telemetry/weaver).

Anything in this package that isn't in `{{RootNamespace}}` is not yet stable and may change or be removed in a later release.

- Stable definitions are marked `[Obsolete]` and point to their home in `{{RootNamespace}}`, so your code keeps compiling when a convention stabilises.
- Deprecated definitions are marked `[Obsolete]` with the reason from the registry.

```csharp
using {{RootNamespace}}.Incubating;

activity?.SetTag(GenAiIncubatingAttributes.GenAiRequestModel, "gpt-4o");
```
