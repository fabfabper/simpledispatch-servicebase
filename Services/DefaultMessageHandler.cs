using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using SimpleDispatch.ServiceBase.Interfaces;

namespace SimpleDispatch.ServiceBase.Services;

/// <summary>
/// Default message handler implementation that can be overridden in derived services
/// </summary>
public class DefaultMessageHandler : IMessageHandler
{
    private readonly ILogger<DefaultMessageHandler> _logger;

    public DefaultMessageHandler(ILogger<DefaultMessageHandler> logger)
    {
        _logger = logger;
    }

    public virtual async Task HandleMessageAsync(string message, BasicDeliverEventArgs args)
    {
        _logger.LogInformation("Processing message: {Message}", message);
        
        // Default implementation - override this method in your derived services
        // to implement custom message handling logic
        
        await Task.CompletedTask;
        
        _logger.LogInformation("Message processed successfully");
    }
}
