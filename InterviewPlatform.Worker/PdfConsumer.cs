using System.Text.Json;
using InterviewPlatform.Shared;

namespace InterviewPlatform.Worker;

public class PdfConsumer : KafkaConsumerService
{
    private readonly IPdfGenerator _generator;
    private readonly IDocumentStore _store;

    public PdfConsumer(
        IConfiguration config,
        ILogger<PdfConsumer> logger,
        IPdfGenerator generator,
        IDocumentStore store)
        : base(config, logger, "Kafka:PdfTopic", "Kafka:PdfGroupId")
    {
        _generator = generator;
        _store = store;
    }

    protected override async Task HandleAsync(string json)
    {
        PdfRequestMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<PdfRequestMessage>(json);
        }
        catch (JsonException ex)
        {
            Logger.LogError(ex, "Некорректное сообщение в топике PDF");
            return;
        }

        if (message == null)
        {
            return;
        }

        // ошибка в одном документе не должна останавливать Worker
        try
        {
            var pdf = _generator.Generate(message.Type, message.Data);
            await _store.MarkReadyAsync(message.RequestId, pdf);
            Logger.LogInformation("PDF {RequestId} готов, размер {Size} байт", message.RequestId, pdf.Length);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Не удалось создать PDF {RequestId}", message.RequestId);
            await _store.MarkFailedAsync(message.RequestId, ex.Message);
        }
    }
}
