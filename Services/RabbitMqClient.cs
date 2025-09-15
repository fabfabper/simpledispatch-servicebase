using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleDispatch.ServiceBase.Configuration;
using SimpleDispatch.ServiceBase.Interfaces;
using System.Text;

namespace SimpleDispatch.ServiceBase.Services;

/// <summary>
/// RabbitMQ client implementation for message consumption and publishing
/// 
/// This implementation uses IServiceProvider to resolve IMessageHandler for each message
/// to avoid the DI anti-pattern where a singleton service (IRabbitMqClient) would directly
/// inject a scoped service (IMessageHandler). Each message is processed in its own DI scope,
/// ensuring proper lifecycle management for DbContext and other scoped services.
/// </summary>
public class RabbitMqClient : IRabbitMqClient
{
    private readonly ILogger<RabbitMqClient> _logger;
    private readonly RabbitMqSettings _settings;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed = false;

    public RabbitMqClient(
        ILogger<RabbitMqClient> logger,
        IOptions<RabbitMqSettings> settings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
    }

    public async Task StartConsumingAsync()
    {
        try
        {
            await InitializeConnectionAsync();
            
            if (_channel == null)
            {
                throw new InvalidOperationException("Channel is not initialized");
            }

            _channel.QueueDeclare(
                queue: _settings.QueueName,
                durable: _settings.Durable,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            if (!string.IsNullOrEmpty(_settings.ExchangeName))
            {
                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: _settings.ExchangeType,
                    durable: _settings.Durable);

                _channel.QueueBind(
                    queue: _settings.QueueName,
                    exchange: _settings.ExchangeName,
                    routingKey: "");
            }

            _channel.BasicQos(0, _settings.PrefetchCount, false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                // Create a new scope for each message to ensure proper DI lifecycle
                using var scope = _serviceProvider.CreateScope();
                
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Received message: {Message}", message);

                    // Resolve the message handler from the scoped service provider
                    var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
                    await messageHandler.HandleMessageAsync(message, ea);

                    if (!_settings.AutoAck)
                    {
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    
                    if (!_settings.AutoAck)
                    {
                        _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }
                }
            };

            _channel.BasicConsume(
                queue: _settings.QueueName,
                autoAck: _settings.AutoAck,
                consumer: consumer);

            _logger.LogInformation("Started consuming messages from queue: {QueueName}", _settings.QueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start consuming messages");
            throw;
        }
    }

    public async Task StopConsumingAsync()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _logger.LogInformation("Stopped consuming messages");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping message consumption");
        }
    }

    public async Task PublishMessageAsync(string message, string routingKey = "")
    {
        try
        {
            await InitializeConnectionAsync();

            if (_channel == null)
            {
                throw new InvalidOperationException("Channel is not initialized");
            }

            if (!string.IsNullOrEmpty(_settings.ExchangeName))
            {
                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: _settings.ExchangeType,
                    durable: _settings.Durable);
            }

            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = _settings.Durable;

            _channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published message to exchange: {Exchange}, routing key: {RoutingKey}", 
                _settings.ExchangeName, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message");
            throw;
        }
    }

    private async Task InitializeConnectionAsync()
    {
        if (_connection != null && _connection.IsOpen)
            return;

        var factory = new ConnectionFactory()
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _logger.LogInformation("Connected to RabbitMQ at {HostName}:{Port}", _settings.HostName, _settings.Port);
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ client");
        }

        _disposed = true;
    }
}
