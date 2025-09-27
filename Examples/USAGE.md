# Example Usage

This directory contains example code showing how to use the SimpleDispatch.ServiceBase package to create microservices.

## Example 1: Simple Order Processing Service

### Project Structure

```
OrderProcessingService/
├── Program.cs
├── OrderProcessingService.csproj
├── appsettings.json
├── Services/
│   └── OrderMessageHandler.cs
├── Controllers/
│   └── OrderController.cs
└── Models/
    └── Order.cs
```

### Program.cs

```csharp
using OrderProcessingService;

var service = new OrderService(args);
await service.RunAsync();
```

### OrderProcessingService.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="SimpleDispatch.ServiceBase" Version="1.0.0" />
  </ItemGroup>

</Project>
```

### OrderService.cs

```csharp
using SimpleDispatch.ServiceBase;
using SimpleDispatch.ServiceBase.Interfaces;
using OrderProcessingService.Services;
using Microsoft.Extensions.Configuration;
using SimpleDispatch.ServiceBase.Extensions;

namespace OrderProcessingService;

public class OrderService : BaseService
{
    public OrderService(string[] args) : base(args) { }

    protected override void ConfigureServices()
    {
        // Add any additional services specific to order processing
        Builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        Builder.Services.AddScoped<INotificationService, NotificationService>();
        // Register the RabbitMQ producer with configuration
        Builder.Services.AddRabbitMqProducer(Builder.Configuration);
        // Register the manual RabbitMQ consumer as a BackgroundService
        Builder.Services.AddHostedService<OrderMessageConsumerService>();
    }
}
```

### OrderMessageConsumerService.cs (Manual Consumer)

```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProcessingService.Models;
using SimpleDispatch.ServiceBase.Interfaces;

public class OrderMessageConsumerService : BackgroundService
{
    private readonly IRabbitMqClient _rabbitMqClient;
    private readonly ILogger<OrderMessageConsumerService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OrderMessageConsumerService(IRabbitMqClient rabbitMqClient, ILogger<OrderMessageConsumerService> logger, IServiceProvider serviceProvider)
    {
        _rabbitMqClient = rabbitMqClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting manual RabbitMQ consumer loop...");
        await _rabbitMqClient.ConsumeAsync(async (message, args) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            _logger.LogInformation("Received order message: {Message}", message);
            var order = JsonConvert.DeserializeObject<Order>(message);
            if (order == null)
            {
                _logger.LogWarning("Failed to deserialize order message");
                return;
            }
            // Process the order
            await orderRepository.SaveOrderAsync(order);
            // Send notification
            await notificationService.SendOrderConfirmationAsync(order);
            _logger.LogInformation("Order processed successfully: {OrderId}", order.Id);
        }, stoppingToken);
    }
}
```

### OrderController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using SimpleDispatch.ServiceBase.Controllers;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Models;
using OrderProcessingService.Models;
using Newtonsoft.Json;

namespace OrderProcessingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : BaseApiController
{
    private readonly IOrderRepository _orderRepository;
    private readonly IRabbitMqProducer _producer;

    public OrderController(
        IRabbitMqClient rabbitMqClient,
        IOrderRepository orderRepository,
        IRabbitMqProducer producer) : base(rabbitMqClient)
    {
        _orderRepository = orderRepository;
        _producer = producer;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Order>>> CreateOrder([FromBody] Order order)
    {
        try
        {
            // Save order to database
            var savedOrder = await _orderRepository.SaveOrderAsync(order);

            // Publish order created event
            var message = JsonConvert.SerializeObject(savedOrder);
            // Use the producer to publish
            await _producer.PublishAsync(message, "order.created");

            return Ok(ApiResponse<Order>.CreateSuccess(savedOrder, "Order created and published successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Order>.CreateError(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Order>>> GetOrder(int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderAsync(id);
            if (order == null)
            {
                return NotFound(ApiResponse<Order>.CreateError("Order not found"));
            }

            return Ok(ApiResponse<Order>.CreateSuccess(order));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<Order>.CreateError(ex.Message));
        }
    }
}
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "QueueName": "orders.processing",
    "ExchangeName": "orders.exchange",
    "ExchangeType": "topic",
    "Durable": true,
    "AutoAck": false,
    "PrefetchCount": 5
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrderProcessing;Trusted_Connection=true;"
  },
  "RabbitMqProducer": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "ExchangeName": "orders.exchange",
    "ExchangeType": "topic",
    "Durable": true,
    "AutoDelete": false,
    "PersistentMessages": true
  }
}
```

## Running the Service

1. Ensure RabbitMQ is running on your machine
2. Update the connection strings in `appsettings.json`
3. Run the service: `dotnet run`
4. Access Swagger UI at: `https://localhost:5001/swagger`
5. Check health endpoint: `https://localhost:5001/health`

## Key Benefits

- **Minimal Boilerplate**: The base service handles all the infrastructure setup
- **Manual RabbitMQ Consumer**: Use BackgroundService for robust, explicit message consumption
- **Extensible**: Easy to add custom consumers, producers, and services
- **Production Ready**: Includes logging, health checks, and error handling
- **Testable**: Clean separation of concerns makes unit testing straightforward
