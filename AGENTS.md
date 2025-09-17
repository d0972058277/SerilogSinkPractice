# Repository Guidelines

## Project Structure & Module Organization
SerilogSinkPractice.sln anchors a single C# console app in `SerilogSinkDemo/`. `Program.cs` bootstraps Serilog sinks and should remain the orchestrator for pipeline wiring. `appsettings.json` carries sink credentials and toggleable enrichers. Generated logs land in `SerilogSinkDemo/logs/`. Observability infrastructure lives under `logstash/` (Logstash configs) and top-level `docker-compose.yml` for Kafka, Seq, Grafana, and Loki services.

## Build, Test, and Development Commands
- `dotnet restore SerilogSinkPractice.sln` — restore NuGet packages before the first build.
- `dotnet build SerilogSinkPractice.sln` — compile the console app and validate dependency graph.
- `dotnet run --project SerilogSinkDemo/SerilogSinkDemo.csproj` — execute the sample producer with the configured sinks.
- `docker-compose up -d` — start the ELK/Kafka observability stack; pair with `docker-compose logs -f <service>` for troubleshooting.

## Coding Style & Naming Conventions
Use 4-space indentation and UTF-8 source files without BOM. Follow standard C# casing (`PascalCase` for classes, `camelCase` for locals, `ALL_CAPS` only for constants that map to environment variables). Prefer expression-bodied members for simple delegates, but keep logging pipelines explicit for readability. Keep Serilog configuration fluent chains aligned per call, one sink per line, and mirror property names between enrichers and downstream consumers.

## Testing Guidelines
New coverage should live in an `SerilogSinkDemo.Tests` xUnit project under a top-level `tests/` folder. Name test files `<Target>Tests.cs` and methods `MethodUnderTest_Result_Condition`. Run `dotnet test` from the repo root; add `--logger trx` when CI artifacts are needed. Aim to exercise custom sinks and enrichers with integration-style tests that assert on structured payloads or emitted files.

## Commit & Pull Request Guidelines
Commits in this repo use concise, imperative summaries (`Integrate Grafana...`, `Add File sink...`). Match that style, limit subject lines to ~72 characters, and batch related edits together. Pull requests should describe observable behavior changes, list any new configuration keys, and include screenshots or log samples when altering dashboards. Link issues when available and note required follow-up work explicitly in the description.

## Observability & Configuration Notes
Keep secrets out of `appsettings.json`; rely on `dotnet user-secrets` or environment variables in deployment scenarios. When updating Logstash pipelines, sync changes with `logstash/pipeline/logstash.conf` and document offsets in the ingress broker. Clean up local data stores via `docker-compose down --volumes` once experiments finish to reclaim disk space.
