using Serilog;
using Serilog.Events;
using Elastic.Ingest.Elasticsearch;
using Elastic.Serilog.Sinks;
using Serilog.Sinks.Grafana.Loki;

var kafkaSink = new KafkaSink("localhost:9092", "serilog-logs");

// ===== Serilog 設定 =====
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)

    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentUserName()
    .Enrich.WithProperty("ProcessId", Environment.ProcessId)

    // Console（方便檢視 s_*）
    .WriteTo.Console(outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {s_type} {s_event} {Message:lj} {Properties:j}{NewLine}{Exception}")

    // === File ===
    .WriteTo.File("logs/event-logs-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {s_type} {s_event} {Message:lj} {Properties:j}{NewLine}{Exception}")
    
    // === Elasticsearch ===
    .WriteTo.Elasticsearch(
        [new Uri("http://localhost:9200")],
        opts =>opts.BootstrapMethod = BootstrapMethod.None
        )
    
    // === Kafka ===
    .WriteTo.Sink(kafkaSink)

    // === Loki ===
    .WriteTo.GrafanaLoki(
        "http://localhost:3100",
        labels:[
        new LokiLabel { Key = "app", Value = "serilog-demo" },
        new LokiLabel { Key = "environment", Value = "development" }
    ])

    // === Seq ===
    .WriteTo.Seq("http://localhost:5341")

    .CreateLogger();

try
{
    // ===== 業務日誌測試 =====

    // 事件日誌測試
    Log.ForContext("s_type", "EventLog")
       .Information("Application started successfully");

    // 用戶操作日誌
    Log.ForContext("s_event", "UserLogin")
       .Information("User authenticated: {UserId}", "user123");

    // 訂單處理日誌
    Log.ForContext("s_topic", "orders")
       .Information("Order processing initiated: {OrderId}", "ORD-001");

    // 分區處理日誌
    Log.ForContext("s_partition", "partition-1")
       .Information("Message routed to partition for processing");

    // 完整業務事件
    Log.ForContext("s_type", "EventLog")
       .ForContext("s_event", "OrderCreated")
       .ForContext("s_topic", "orders")
       .Information("New order created: {OrderId}, Amount: {Amount:C}", "ORD-002", 99.99m);

    // 一般系統日誌
    Log.Information("System health check completed");

    // 配置驗證日誌
    Log.ForContext("s_event", "")
       .ForContext("s_topic", "   ")
       .Information("Configuration validation: Empty properties handled correctly");

    // 系統日誌類型測試
    Log.ForContext("s_type", "SystemLog")
       .Information("System monitoring data collected");

    Console.WriteLine("=== Starting business logic simulation ===");

    var tasks = new List<Task>();

    for (int i = 0; i < 5; i++)
    {
        int index = i;
        tasks.Add(Task.Run(async () =>
        {
            await Task.Delay(Random.Shared.Next(100, 500));

            // 業務事件日誌
            Log.ForContext("s_type", "EventLog")
               .ForContext("s_event", "ItemProcessingStarted")
               .Information("Processing item {ItemIndex} on thread {ThreadId}",
                   index,
                   Thread.CurrentThread.ManagedThreadId);

            if (index % 2 == 0)
            {
                // 訊息佇列事件
                Log.ForContext("s_topic", $"item-events")
                   .ForContext("s_partition", $"partition-{index % 3}")
                   .Warning("Kafka message sent for item {ItemIndex}", index);
            }

            if (index % 3 == 0)
            {
                // 系統錯誤處理
                Log.Error("System error processing item {ItemIndex}", index);
            }

            // 業務完成事件
            Log.ForContext("s_type", "EventLog")
               .ForContext("s_event", "ItemProcessingCompleted")
               .Information("Completed processing item {ItemIndex}", index);
        }));
    }

    await Task.WhenAll(tasks);

    // 任務完成通知
    Log.ForContext("s_type", "EventLog")
       .ForContext("s_event", "AllTasksCompleted")
       .Information("All business tasks completed successfully");

    // 系統狀態通知
    Log.Information("Application demo completed successfully. Logs are available across multiple sinks for analysis.");

    Console.WriteLine("=== Demo completed successfully ===\n");
    Console.WriteLine("Log destinations:");
    Console.WriteLine("📁 File logs: logs/event-logs-*.log (business events with s_type/s_event/s_topic/s_partition)");
    Console.WriteLine("🔍 Elasticsearch: http://localhost:9200 (system logs and general events)");
    Console.WriteLine("📊 Kibana: http://localhost:5601 (Elasticsearch log visualization)");
    Console.WriteLine("📊 Seq Web UI: http://localhost:5341 (structured logs with advanced search)");
    Console.WriteLine("📈 Grafana/Loki: http://localhost:3000 (log visualization and monitoring)");
    Console.WriteLine("⚡ Kafka UI: http://localhost:8081 (view Kafka topics and messages)");
    Console.WriteLine("⚡ Kafka: Topic 'serilog-logs' (real-time log streaming)");
    Console.WriteLine("\n💡 Console output shows all logs with enhanced formatting");
    Console.WriteLine("\n按任意鍵結束程式...");
    Console.ReadKey();
}
catch (Exception ex)
{
    // 系統致命錯誤
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
    kafkaSink?.Dispose();
}
