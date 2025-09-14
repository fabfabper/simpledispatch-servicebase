using RabbitMQ.Client.Events;

namespace SimpleDispatch.ServiceBase.Interfaces;

/// <summary>
/// Interface for handling messages from RabbitMQ queue
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Process a message received from the queue
    /// </summary>
    /// <param name="message">The message content as string</param>
    /// <param name="args">RabbitMQ event arguments</param>
    /// <returns>Task representing the async operation</returns>
    Task HandleMessageAsync(string message, BasicDeliverEventArgs args);
}
