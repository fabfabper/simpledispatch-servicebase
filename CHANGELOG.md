# Changelog

All notable changes to SimpleDispatch.ServiceBase will be documented in this file.

## [1.3.0] - 2024-09-15

### Added

- **API Port Configuration**: Microservices can now configure HTTP and HTTPS ports
- **ApiSettings Configuration Class**: New configuration section for API server settings
- **Port Configuration Methods**: Added `ConfigureHttpPort()`, `ConfigureHttpPorts()`, and `ConfigureUrls()` methods
- **Flexible URL Configuration**: Support for custom URLs and binding addresses
- **Production Swagger Control**: Option to enable/disable Swagger in production environments
- **HTTPS Redirection Control**: Configurable HTTPS redirection behavior

### API Configuration Options

- `HttpPort`: Configure HTTP server port
- `HttpsPort`: Configure HTTPS server port
- `Urls`: Specify custom URLs to listen on
- `EnableHttpsRedirection`: Control HTTPS redirection (default: true)
- `EnableSwaggerInProduction`: Enable Swagger in production (default: false)

### Usage Examples

```csharp
// In appsettings.json
{
  "Api": {
    "HttpPort": 8080,
    "HttpsPort": 8443,
    "EnableHttpsRedirection": true
  }
}

// Or programmatically
public class MyService : BaseService
{
    public MyService(string[] args) : base(args)
    {
        ConfigureHttpPort(8080);
        // or ConfigureHttpPorts(8080, 8443);
        // or ConfigureUrls("http://0.0.0.0:8080");
    }
}
```

## [1.2.1] - 2024-09-15

### Fixed

- **Critical DI Scoping Issue**: Fixed dependency injection anti-pattern where `IRabbitMqClient` (singleton) was directly injecting `IMessageHandler` (scoped service)
- **DbContext Scope Issues**: Resolved issues with DbContext and other scoped services being accessed from singleton context
- **Message Handler Lifetime**: Each message now gets processed in its own dependency injection scope, ensuring proper lifecycle management

### Changed

- **RabbitMqClient Implementation**: Now uses `IServiceProvider` to create scoped service instances for each message
- **Dependency Resolution**: Message handlers are now resolved per-message rather than injected at construction time

### Technical Details

- The `RabbitMqClient` constructor now accepts `IServiceProvider` instead of `IMessageHandler`
- Each incoming message creates a new DI scope using `_serviceProvider.CreateScope()`
- Message handlers and their dependencies (including DbContext) are resolved within the message-specific scope
- Automatic scope disposal ensures proper cleanup after message processing

## [1.2.0] - 2024-09-14

### Changed

- **Publishing Target**: Package now published to GitHub Packages instead of NuGet.org
- **CI/CD Workflow**: Updated to use GitHub Packages with automatic GITHUB_TOKEN authentication
- **Installation Process**: Updated documentation with GitHub Packages configuration instructions
- **Repository References**: Made GitHub repository references generic for easier forking

### Removed

- **Tag-based Release Workflow**: Simplified to push-to-main only deployment
- **NuGet.org Publishing**: No longer publishing to public NuGet repository
- **Manual Release Creation**: Removed automated GitHub release creation

### Documentation

- **README**: Updated with GitHub Packages installation instructions
- **CICD_SETUP**: Simplified setup process for GitHub Packages
- **DEPLOYMENT_CHECKLIST**: Updated deployment steps and troubleshooting

## [1.1.0] - 2024-09-14

### Added

- **PostgreSQL Database Support**: Full Entity Framework Core integration
- **Repository Pattern**: Base repository with CRUD operations and generic interface
- **Unit of Work Pattern**: Transaction management for database operations
- **Database Configuration**: Configurable PostgreSQL settings with retry policies
- **Base DbContext**: Abstract DbContext with entity configuration support
- **Database Extension Methods**: Easy database service registration
- **Example Database Implementation**: Complete example with entities, repositories, and controllers

### Enhanced

- **BaseService**: Added database configuration support
- **Example Microservice**: Updated to demonstrate database usage
- **Documentation**: Comprehensive database usage examples and configuration
- **API Controllers**: Added database operations examples

### Features

- **Entity Framework Core**: Latest EF Core with PostgreSQL provider
- **Connection Resilience**: Automatic retry policies for database operations
- **Transaction Support**: Built-in transaction management with Unit of Work
- **Repository Pattern**: Generic repository with common CRUD operations
- **Configuration Management**: Database settings through appsettings.json

## [1.0.0] - 2024-09-14

### Added

- Initial release of SimpleDispatch.ServiceBase
- RabbitMQ client with automatic connection management
- Extensible message handler interface for custom message processing
- Base service class with ASP.NET Core integration
- Default controllers for messaging operations
- Health check endpoints
- Swagger/OpenAPI documentation support
- CORS configuration
- Comprehensive logging support
- Configuration management for RabbitMQ settings
- Extension methods for easy service registration
- Example implementations and documentation

### Features

- **RabbitMQ Integration**: Built-in RabbitMQ client for message consumption and publishing
- **Extensible Architecture**: Override message handlers to implement custom business logic
- **REST API Foundation**: Pre-configured ASP.NET Core setup with controllers and Swagger
- **Health Monitoring**: Built-in health check endpoints for monitoring
- **Configuration**: Easy configuration through appsettings.json
- **Logging**: Structured logging with configurable levels
- **CORS Support**: Pre-configured CORS for cross-origin requests
