# Changelog

All notable changes to SimpleDispatch.ServiceBase will be documented in this file.

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
