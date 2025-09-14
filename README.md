# SimpleDispatch.ServiceBase

A NuGet package that provides a base foundation for SimpleDispatch microservices with RabbitMQ messaging and REST API capabilities.

## Features

- **RabbitMQ Integration**: Built-in RabbitMQ client for message consumption and publishing
- **PostgreSQL Database**: Entity Framework Core integration with repository pattern
- **Extensible Message Handling**: Override message handlers to implement custom business logic
- **REST API Foundation**: Pre-configured ASP.NET Core setup with controllers, Swagger, and health checks
- **Repository Pattern**: Base repository with CRUD operations and Unit of Work pattern
- **Transaction Management**: Built-in transaction support for database operations
- **Configuration Management**: Easy configuration through appsettings.json
- **Logging**: Structured logging with configurable levels
- **Health Checks**: Built-in health check endpoints
- **CORS Support**: Pre-configured CORS for cross-origin requests

## Installation

This package is published to GitHub Packages. You'll need to configure your NuGet.config to access it:

### 1. Create or update NuGet.config

Create a `NuGet.config` file in your project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/[YOUR_GITHUB_USERNAME]/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

### 2. Install the package

```bash
dotnet add package SimpleDispatch.ServiceBase
```

> **Note**: You'll need a GitHub Personal Access Token with `read:packages` permission to install packages from GitHub Packages.

## Quick Start

### 1. Create a new microservice

```csharp
using SimpleDispatch.ServiceBase;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Database;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client.Events;

public class MyMicroservice : BaseService
{
    public MyMicroservice(string[] args) : base(args)
    {
    }

    protected override void RegisterMessageHandler()
    {
        Builder.Services.AddScoped<IMessageHandler, MyMessageHandler>();
    }

    protected override void ConfigureDatabase()
    {
        ConfigurePostgreSqlDbContext<MyDbContext>();
    }

    protected override void ConfigureServices()
    {
        Builder.Services.AddScoped<IMyRepository, MyRepository>();
    }
}

public class MyDbContext : BaseDbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    public DbSet<MyEntity> MyEntities { get; set; } = null!;

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MyEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });
    }
}

public class MyMessageHandler : IMessageHandler
{
    private readonly ILogger<MyMessageHandler> _logger;
    private readonly IMyRepository _repository;

    public MyMessageHandler(ILogger<MyMessageHandler> logger, IMyRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task HandleMessageAsync(string message, BasicDeliverEventArgs args)
    {
        _logger.LogInformation("Processing custom message: {Message}", message);

        // Implement your custom message handling logic here
        var entity = new MyEntity { Name = message };
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();

        await Task.CompletedTask;
    }
}

// Program.cs
public class Program
{
    public static async Task Main(string[] args)
    {
        var service = new MyMicroservice(args);
        await service.RunAsync();
    }
}
```

### 2. Configure database and RabbitMQ settings

Add the following to your `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=myservice;Username=postgres;Password=postgres"
  },
  "Database": {
    "MaxRetryCount": 3,
    "MaxRetryDelay": 30,
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  },
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "QueueName": "your.queue.name",
    "ExchangeName": "your.exchange.name",
    "ExchangeType": "direct",
    "Durable": true,
    "AutoAck": false,
    "PrefetchCount": 1
  }
}
```

### 3. Create repositories and controllers

```csharp
using Microsoft.AspNetCore.Mvc;
using SimpleDispatch.ServiceBase.Controllers;
using SimpleDispatch.ServiceBase.Database;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Models;

// Repository interface
public interface IMyRepository : IRepository<MyEntity, int>
{
    Task<IEnumerable<MyEntity>> GetByNameAsync(string name);
}

// Repository implementation
public class MyRepository : BaseRepository<MyEntity, int, MyDbContext>, IMyRepository
{
    public MyRepository(MyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MyEntity>> GetByNameAsync(string name)
    {
        return await FindAsync(e => e.Name.Contains(name));
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class MyController : BaseApiController
{
    private readonly IMyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public MyController(
        IRabbitMqClient rabbitMqClient,
        IMyRepository repository,
        IUnitOfWork unitOfWork) : base(rabbitMqClient)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MyEntity>>> Create([FromBody] CreateMyEntityRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var entity = new MyEntity { Name = request.Name };
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            // Publish event
            await RabbitMqClient.PublishMessageAsync($"Entity created: {entity.Name}", "entity.created");

            await _unitOfWork.CommitTransactionAsync();

            return Ok(ApiResponse<MyEntity>.CreateSuccess(entity, "Entity created successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return StatusCode(500, ApiResponse<MyEntity>.CreateError(ex.Message));
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<MyEntity>>> Get(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound(ApiResponse<MyEntity>.CreateError("Entity not found"));
            }

            return Ok(ApiResponse<MyEntity>.CreateSuccess(entity));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<MyEntity>.CreateError(ex.Message));
        }
    }
}
```

## Quick Links

- 📦 **[NuGet Package](https://www.nuget.org/packages/SimpleDispatch.ServiceBase/)**
- 🔧 **[CI/CD Setup Guide](CICD_SETUP.md)** - Automated publishing with GitHub Actions
- 📖 **[Usage Examples](Examples/USAGE.md)** - Complete implementation examples
- 📋 **[Changelog](CHANGELOG.md)** - Version history and release notes

## Configuration Options

### RabbitMQ Settings

| Property        | Description                                    | Default     |
| --------------- | ---------------------------------------------- | ----------- |
| `HostName`      | RabbitMQ server hostname                       | `localhost` |
| `Port`          | RabbitMQ server port                           | `5672`      |
| `UserName`      | Username for authentication                    | `guest`     |
| `Password`      | Password for authentication                    | `guest`     |
| `VirtualHost`   | Virtual host to connect to                     | `/`         |
| `QueueName`     | Queue name for consuming messages              | Required    |
| `ExchangeName`  | Exchange name for publishing                   | Optional    |
| `ExchangeType`  | Exchange type (direct, topic, fanout, headers) | `direct`    |
| `Durable`       | Whether the queue should be durable            | `true`      |
| `AutoAck`       | Whether to auto-acknowledge messages           | `false`     |
| `PrefetchCount` | Prefetch count for message consumption         | `1`         |

### Database Settings

| Property                     | Description                               | Default  |
| ---------------------------- | ----------------------------------------- | -------- |
| `ConnectionString`           | PostgreSQL connection string              | Required |
| `MaxRetryCount`              | Maximum number of retry attempts          | `3`      |
| `MaxRetryDelay`              | Maximum delay between retries (seconds)   | `30`     |
| `EnableSensitiveDataLogging` | Enable sensitive data logging (dev only)  | `false`  |
| `EnableDetailedErrors`       | Enable detailed error messages (dev only) | `false`  |
| `CommandTimeout`             | Command timeout in seconds                | `30`     |

## Extension Methods

The package provides extension methods for easier configuration:

```csharp
using SimpleDispatch.ServiceBase.Extensions;

// In your Program.cs or Startup.cs
builder.Services.AddSimpleDispatchBase(options =>
{
    options.HostName = "rabbitmq-server";
    options.QueueName = "my-queue";
});

// Add PostgreSQL database support
builder.Services.AddPostgreSqlDatabase<MyDbContext>(configuration);

// Add a custom message handler
builder.Services.AddMessageHandler<MyCustomMessageHandler>();

// Add repositories
builder.Services.AddRepository<IMyRepository, MyRepository>();
```

## API Endpoints

The base service automatically provides the following endpoints:

- `GET /health` - Health check endpoint
- `GET /api/messaging/health` - Messaging service health check
- `POST /api/messaging/publish` - Publish a message to RabbitMQ

## Dependencies

- .NET 9.0
- ASP.NET Core
- Entity Framework Core
- PostgreSQL (via Npgsql.EntityFrameworkCore.PostgreSQL)
- RabbitMQ.Client
- Microsoft.Extensions.Hosting
- Newtonsoft.Json

## License

MIT License
