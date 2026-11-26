using Confluent.Kafka;

namespace InterviewPlatform.Worker;

// общая часть для всех потребителей Kafka: подключение и цикл чтения
public abstract class KafkaConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly string _topicKey;
    private readonly string _groupKey;
    protected readonly ILogger Logger;

    protected KafkaConsumerService(IConfiguration config, ILogger logger, string topicKey, string groupKey)
    {
        _config = config;
        Logger = logger;
        _topicKey = topicKey;
        _groupKey = groupKey;
    }

    // Consume() блокирует поток, поэтому запускаем цикл отдельно
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => RunAsync(stoppingToken), stoppingToken);
    }

    // обработка одного сообщения (JSON)
    protected abstract Task HandleAsync(string json);

    private async Task RunAsync(CancellationToken token)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = _config[_groupKey],
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = true
        };

        var topic = _config[_topicKey]!;

        using var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        consumer.Subscribe(topic);
        Logger.LogInformation("Слушаю топик {Topic}", topic);

        while (!token.IsCancellationRequested)
        {
            ConsumeResult<Ignore, string>? result;

            try
            {
                result = consumer.Consume(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                Logger.LogError(ex, "Ошибка чтения из Kafka");
                await Task.Delay(2000);
                continue;
            }

            if (result?.Message == null)
            {
                continue;
            }

            await HandleAsync(result.Message.Value);
        }

        consumer.Close();
    }
}
