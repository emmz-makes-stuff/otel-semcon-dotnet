# otel-semcon-dotnet

[OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/) for .NET, generated with [OpenTelemetry Weaver](https://github.com/open-telemetry/weaver) following the [code generation guidance](https://opentelemetry.io/docs/specs/semconv/non-normative/code-generation/).

| Package | Contents |
|---|---|
| `Nova.OpenTelemetry.SemanticConventions` | Stable conventions only |
| `Nova.OpenTelemetry.SemanticConventions.Incubating` | All conventions, including experimental and deprecated ones |

Each package has attribute keys (`HttpAttributes.HttpRequestMethod`), well-known values (`HttpAttributes.HttpRequestMethodValues.Get`), metric metadata (`HttpMetrics.ServerRequestDuration.Name`) and instrument factories (`HttpMetrics.CreateServerRequestDuration(meter)`). They target `netstandard2.0`, `net8.0` and `net10.0`.

The package version matches the semantic conventions release it was generated from.

## Regenerating

Requires Docker and `jq`.

```sh
./scripts/generate.sh
dotnet test
```

- The semantic conventions version is `<SemanticConventionsVersion>` in `Directory.Build.props`.
- The Weaver image is pinned in `scripts/generate.sh`.
- Templates are in `templates/registry/dotnet`.
- Generated code is committed under `src/*/Generated`.
