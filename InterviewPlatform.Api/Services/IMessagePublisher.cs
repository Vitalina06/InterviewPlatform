namespace InterviewPlatform.Api.Services;

public interface IMessagePublisher
{
    Task PublishAsync(string topic, object message);
}
