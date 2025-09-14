namespace SimpleDispatch.ServiceBase.Configuration;

/// <summary>
/// Configuration settings for RabbitMQ connection and queue
/// </summary>
public class RabbitMqSettings
{
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// RabbitMQ server hostname
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ server port
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Username for RabbitMQ authentication
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password for RabbitMQ authentication
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual host to connect to
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Queue name to consume messages from
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    /// Exchange name for publishing messages
    /// </summary>
    public string ExchangeName { get; set; } = string.Empty;

    /// <summary>
    /// Exchange type (direct, topic, fanout, headers)
    /// </summary>
    public string ExchangeType { get; set; } = "direct";

    /// <summary>
    /// Whether the queue should be durable
    /// </summary>
    public bool Durable { get; set; } = true;

    /// <summary>
    /// Whether to auto-acknowledge messages
    /// </summary>
    public bool AutoAck { get; set; } = false;

    /// <summary>
    /// Prefetch count for message consumption
    /// </summary>
    public ushort PrefetchCount { get; set; } = 1;
}
