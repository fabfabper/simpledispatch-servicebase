using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using SimpleDispatch.ServiceBase.Configuration;
using SimpleDispatch.ServiceBase.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDispatch.ServiceBase.Services;

public class RabbitMqProducer : IRabbitMqProducer, IDisposable
{
    private readonly ILogger<RabbitMqProducer> _logger;
    private readonly RabbitMqProducerSettings _settings;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _disposed;

    public RabbitMqProducer(ILogger<RabbitMqProducer> logger, IOptions<RabbitMqProducerSettings> options)
    {
        _logger = logger;
        _settings = options.Value;

        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_settings.ExchangeName, _settings.ExchangeType, _settings.Durable, _settings.AutoDelete);
    }

    public Task PublishAsync(string message, string routingKey)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var properties = _channel.CreateBasicProperties();
        properties.Persistent = _settings.PersistentMessages;

        _channel.BasicPublish(
            exchange: _settings.ExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body
        );
        _logger.LogInformation("Published message to exchange '{Exchange}' with routing key '{RoutingKey}'", _settings.ExchangeName, routingKey);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}