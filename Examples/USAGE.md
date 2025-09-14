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

namespace OrderProcessingService;

public class OrderService : BaseService
{
    public OrderService(string[] args) : base(args)
    {
    }

    protected override void RegisterMessageHandler()
    {
        Builder.Services.AddScoped<IMessageHandler, OrderMessageHandler>();
    }

    protected override void ConfigureServices()
    {
        // Add any additional services specific to order processing
        Builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        Builder.Services.AddScoped<INotificationService, NotificationService>();
    }
}
```

### OrderMessageHandler.cs

```csharp
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using SimpleDispatch.ServiceBase.Interfaces;
using Newtonsoft.Json;
using OrderProcessingService.Models;

namespace OrderProcessingService.Services;

public class OrderMessageHandler : IMessageHandler
{
    private readonly ILogger<OrderMessageHandler> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly INotificationService _notificationService;

    public OrderMessageHandler(
        ILogger<OrderMessageHandler> logger,
        IOrderRepository orderRepository,
        INotificationService notificationService)
    {
        _logger = logger;
        _orderRepository = orderRepository;
        _notificationService = notificationService;
    }

    public async Task HandleMessageAsync(string message, BasicDeliverEventArgs args)
    {
        try
        {
            _logger.LogInformation("Received order message: {Message}", message);

            var order = JsonConvert.DeserializeObject<Order>(message);
            if (order == null)
            {
                _logger.LogWarning("Failed to deserialize order message");
                return;
            }

            // Process the order
            await _orderRepository.SaveOrderAsync(order);

            // Send notification
            await _notificationService.SendOrderConfirmationAsync(order);

            _logger.LogInformation("Order processed successfully: {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order message");
            throw; // Re-throw to trigger message requeue
        }
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

    public OrderController(
        IRabbitMqClient rabbitMqClient,
        IOrderRepository orderRepository) : base(rabbitMqClient)
    {
        _orderRepository = orderRepository;
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
            await RabbitMqClient.PublishMessageAsync(message, "order.created");

            return Ok(ApiResponse<Order>.CreateSuccess(savedOrder, "Order created successfully"));
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
- **Extensible**: Easy to add custom message handlers and services
- **Production Ready**: Includes logging, health checks, and error handling
- **Testable**: Clean separation of concerns makes unit testing straightforward
