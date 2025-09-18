namespace SimpleDispatch.ServiceBase.Configuration;

public class RabbitMqProducerSettings
{
    public const string SectionName = "RabbitMqProducer";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "";
    public string ExchangeType { get; set; } = "direct";
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public bool PersistentMessages { get; set; } = true;
}