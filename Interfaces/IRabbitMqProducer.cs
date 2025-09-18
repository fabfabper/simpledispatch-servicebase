using System.Threading.Tasks;

namespace SimpleDispatch.ServiceBase.Interfaces;

public interface IRabbitMqProducer
{
    Task PublishAsync(string message, string routingKey);
}