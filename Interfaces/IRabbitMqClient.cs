namespace SimpleDispatch.ServiceBase.Interfaces;

/// <summary>
/// Interface for RabbitMQ client wrapper
/// </summary>
public interface IRabbitMqClient : IDisposable
{
    /// <summary>
    /// Start consuming messages from the configured queue
    /// </summary>
    Task StartConsumingAsync();

    /// <summary>
    /// Stop consuming messages
    /// </summary>
    Task StopConsumingAsync();

    /// <summary>
    /// Publish a message to the specified exchange
    /// </summary>
    /// <param name="message">Message content</param>
    /// <param name="routingKey">Routing key for the message</param>
    Task PublishMessageAsync(string message, string routingKey = "");
}
