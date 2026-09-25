# otel-semcon-dotnet

[OpenTelemetry semantic conventions](https://opentelemetry.io/docs/specs/semconv/) for .NET, generated with [OpenTelemetry Weaver](https://github.com/open-telemetry/weaver) following the [code generation guidance](https://opentelemetry.io/docs/specs/semconv/non-normative/code-generation/).

| Package | Contents |
|---|---|
| `Nova.OpenTelemetry.SemanticConventions` | Stable conventions only |
| `Nova.OpenTelemetry.SemanticConventions.Incubating` | All conventions, including experimental and deprecated ones |

Each package has attribute keys (`HttpAttributes.HttpRequestMethod`), well-known values (`HttpAttributes.HttpRequestMethodValues.Get`), metric metadata (`HttpMetrics.ServerRequestDuration.Name`) and instrument factories (`HttpMetrics.CreateServerRequestDuration(meter)`). They target `netstandard2.0`, `net8.0` and `net10.0`.

The package version matches the semantic conventions release it was generated from.

## Building

Requires the .NET 10 SDK, Docker, `jq` and `bash`. On Windows, use WSL or Git Bash.

```sh
dotnet test
dotnet pack -c Release
```

The generated C# is not committed. The build runs `scripts/generate.sh`, which runs Weaver in Docker, whenever the code is missing or out of date. It's out of date when the templates, the script, `Package.props` or the semconv version have changed since the last run. You can also run `scripts/generate.sh` directly.

- The semantic conventions version is `<SemanticConventionsVersion>` in `Directory.Build.props`.
- The Weaver image is pinned in `scripts/generate.sh`.
- Templates are in `templates/registry/dotnet`.

## Publishing your own packages

To publish these packages under your own name, fork or clone this repository and edit **`Package.props`**:

```xml
<SemanticConventionsRootNamespace>Acme.OpenTelemetry.SemanticConventions</SemanticConventionsRootNamespace>
<Authors>Acme</Authors>
<RepositoryUrl>https://github.com/acme/otel-semcon-dotnet</RepositoryUrl>
```

That's the only change needed. The package IDs, assembly names and C# namespaces all come from `SemanticConventionsRootNamespace`, with `.Incubating` appended for the second package. The next build regenerates the code under the new name.

Upstream doesn't change `Package.props`, and the generated code isn't committed, so a fork can keep pulling from upstream without conflicts.
