# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

**Build and Run:**
- `dotnet restore SerilogSinkDemo` - Restore NuGet packages
- `dotnet build SerilogSinkDemo` - Build the project (.NET 9 target)
- `dotnet run --project SerilogSinkDemo` - Run the demo application
- `dotnet format SerilogSinkDemo` - Format code to standard C# style

**Infrastructure:**
- `docker-compose up -d` - Start all logging infrastructure (ELK, Kafka, Grafana/Loki, Seq)
- `docker-compose down -v` - Stop infrastructure and remove volumes
- `curl http://localhost:9200/_cluster/health` - Check Elasticsearch health

**Testing:**
- No automated tests currently exist
- Create xUnit project in `SerilogSinkDemo.Tests/` for new tests
- Use `dotnet test` from solution root once tests are added

## Architecture

This is a Serilog sink demonstration project showcasing multiple logging destinations:

**Core Components:**
- `SerilogSinkDemo/Program.cs` - Main application orchestrating multiple Serilog sinks
- `SerilogSinkDemo/KafkaSink.cs` - Custom Kafka sink using Confluent.Kafka library
- `appsettings.json` - Configuration for Elasticsearch, Loki, and Kafka settings

**Logging Infrastructure:**
- **ELK Stack**: Elasticsearch + Logstash + Kibana for log storage and visualization
- **Kafka**: Message broker for real-time log streaming with Kafka UI
- **Grafana/Loki**: Log aggregation and visualization
- **Seq**: Structured logging platform with web UI
- **File Sink**: Local file logging with daily rotation

**Log Routing Strategy:**
- Custom properties for business event categorization: `s_type`, `s_event`, `s_topic`, `s_partition`
- EventLog vs SystemLog classification
- Enhanced enrichers: MachineName, EnvironmentUserName, ProcessId

**Service Endpoints:**
- Elasticsearch: http://localhost:9200
- Kibana: http://localhost:5601
- Kafka UI: http://localhost:8081
- Grafana: http://localhost:3000
- Seq: http://localhost:5341
- Loki: http://localhost:3100

## Code Conventions

- .NET 9 target framework with nullable reference types enabled
- Standard C# conventions: PascalCase for types/methods, camelCase for locals
- Four-space indentation, braces on new lines
- Serilog configuration grouped under `Serilog` and `Kafka` nodes in appsettings
- Custom sink implementations should implement `ILogEventSink` and `IDisposable`
- Async/await patterns with proper ConfigureAwait usage
- Error handling with try-catch blocks, especially for external service calls

## Project Structure

- `SerilogSinkDemo/` - Console application with all source code
- `logstash/` - Logstash pipeline configuration
- `logs/` - Generated log files (git ignored)
- `docker-compose.yml` - Full infrastructure stack
- `kibana.yml` - Kibana configuration with dark mode enabled by default
- Root-level configuration files for Grafana datasources and Kibana setup