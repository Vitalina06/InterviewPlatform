using System.Text.Json;
using InterviewPlatform.Shared;

namespace InterviewPlatform.Worker;

public class AuditConsumer : KafkaConsumerService
{
    private readonly IAuditStore _store;

    public AuditConsumer(IConfiguration config, ILogger<AuditConsumer> logger, IAuditStore store)
        : base(config, logger, "Kafka:AuditTopic", "Kafka:AuditGroupId")
    {
        _store = store;
    }

    protected override async Task HandleAsync(string json)
    {
        AuditMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<AuditMessage>(json);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Некорректное сообщение в топике аудита");
            return;
        }

        if (message == null)
        {
            return;
        }

        await _store.AddAsync(new AuditRecord
        {
            Action = message.Action,
            UserLogin = message.UserLogin,
            Details = message.Details,
            Time = message.Time
        });

        Logger.LogInformation("Аудит: {Action} ({User})", message.Action, message.UserLogin);
    }
}
