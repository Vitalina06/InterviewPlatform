using Confluent.Kafka;
using InterviewPlatform.Shared;

namespace InterviewPlatform.Api.Services;

public class AuditService : IAuditService
{
    private readonly IMessagePublisher _publisher;
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<AuditService> _logger;
    private readonly string _topic;

    public AuditService(
        IMessagePublisher publisher,
        IHttpContextAccessor http,
        IConfiguration config,
        ILogger<AuditService> logger)
    {
        _publisher = publisher;
        _http = http;
        _logger = logger;
        _topic = config["Kafka:AuditTopic"]!;
    }

    public async Task LogAsync(string action, string details)
    {
        var message = new AuditMessage
        {
            Action = action,
            UserLogin = _http.HttpContext?.User.Identity?.Name ?? "anonymous",
            Details = details,
            Time = DateTime.UtcNow
        };

        // если Kafka недоступна, основное действие пользователя не должно ломаться
        try
        {
            await _publisher.PublishAsync(_topic, message);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Не удалось отправить событие аудита {Action}", action);
        }
    }
}
