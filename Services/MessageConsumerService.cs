using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleDispatch.ServiceBase.Interfaces;

namespace SimpleDispatch.ServiceBase.Services;

/// <summary>
/// Background service that manages RabbitMQ message consumption
/// </summary>
public class MessageConsumerService : BackgroundService
{
    private readonly ILogger<MessageConsumerService> _logger;
    private readonly IRabbitMqClient _rabbitMqClient;

    public MessageConsumerService(
        ILogger<MessageConsumerService> logger,
        IRabbitMqClient rabbitMqClient)
    {
        _logger = logger;
        _rabbitMqClient = rabbitMqClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting message consumer service");
            
            await _rabbitMqClient.StartConsumingAsync();
            
            // Keep the service running until cancellation is requested
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Message consumer service was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in message consumer service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping message consumer service");
        
        try
        {
            await _rabbitMqClient.StopConsumingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping RabbitMQ client");
        }

        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _rabbitMqClient?.Dispose();
        base.Dispose();
    }
}
