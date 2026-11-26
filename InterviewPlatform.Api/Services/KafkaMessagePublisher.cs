using System.Text.Json;
using Confluent.Kafka;

namespace InterviewPlatform.Api.Services;

public class KafkaMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaMessagePublisher> _logger;

    public KafkaMessagePublisher(IConfiguration config, ILogger<KafkaMessagePublisher> logger)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            MessageTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        _logger = logger;
    }

    public async Task PublishAsync(string topic, object message)
    {
        var json = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(topic, new Message<Null, string> { Value = json });
        _logger.LogInformation("Сообщение отправлено в топик {Topic}", topic);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
