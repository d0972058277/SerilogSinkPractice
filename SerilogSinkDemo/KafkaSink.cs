using Confluent.Kafka;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Text;

public class KafkaSink : ILogEventSink, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;
    private readonly JsonFormatter _formatter;

    public KafkaSink(string bootstrapServers, string topic)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            MessageTimeoutMs = 5000,
            RequestTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
        _topic = topic;
        _formatter = new JsonFormatter();
    }

    public void Emit(LogEvent logEvent)
    {
        try
        {
            using var output = new StringWriter();
            _formatter.Format(logEvent, output);
            var message = output.ToString();

            _producer.ProduceAsync(_topic, new Message<Null, string>
            {
                Value = message
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending log to Kafka: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            _producer?.Flush(TimeSpan.FromSeconds(5));
            _producer?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Producer already disposed, ignore
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing Kafka producer: {ex.Message}");
        }
    }
}