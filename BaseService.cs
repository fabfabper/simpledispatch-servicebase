using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleDispatch.ServiceBase.Configuration;
using SimpleDispatch.ServiceBase.Database;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Services;

namespace SimpleDispatch.ServiceBase;

/// <summary>
/// Base service class that provides RabbitMQ messaging and REST API capabilities
/// This class should be inherited by microservices to get base functionality
/// </summary>
public abstract class BaseService
{
    protected WebApplicationBuilder Builder { get; private set; }
    protected WebApplication? App { get; private set; }

    /// <summary>
    /// Create a new base service instance
    /// </summary>
    /// <param name="args">Command line arguments</param>
    protected BaseService(string[] args)
    {
        Builder = WebApplication.CreateBuilder(args);
        ConfigureBaseServices();
    }

    /// <summary>
    /// Configure the base services (RabbitMQ, database, logging, etc.)
    /// </summary>
    private void ConfigureBaseServices()
    {
        // Configure API settings
        Builder.Services.Configure<ApiSettings>(
            Builder.Configuration.GetSection(ApiSettings.SectionName));

        // Configure RabbitMQ settings
        Builder.Services.Configure<RabbitMqSettings>(
            Builder.Configuration.GetSection(RabbitMqSettings.SectionName));

        // Configure Database settings
        Builder.Services.Configure<DatabaseSettings>(
            Builder.Configuration.GetSection(DatabaseSettings.SectionName));

        // Configure API server URLs and ports
        ConfigureApiServer();

        // Register core services
        Builder.Services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
        Builder.Services.AddHostedService<MessageConsumerService>();

        // Register default message handler (can be overridden)
        RegisterMessageHandler();

        // Configure database
        ConfigureDatabase();

        // Add controllers and API support
        Builder.Services.AddControllers();
        Builder.Services.AddEndpointsApiExplorer();
        Builder.Services.AddSwaggerGen();

        // Add CORS support
        Builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add health checks
        Builder.Services.AddHealthChecks();

        // Configure logging
        Builder.Logging.AddConsole();
    }

    /// <summary>
    /// Configure the API server URLs and ports
    /// </summary>
    private void ConfigureApiServer()
    {
        var apiSettings = Builder.Configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>();
        
        if (apiSettings != null)
        {
            var urls = new List<string>();

            // If custom URLs are specified, use them
            if (apiSettings.Urls?.Length > 0)
            {
                urls.AddRange(apiSettings.Urls);
            }
            else
            {
                // Build URLs from port configuration
                if (apiSettings.HttpPort.HasValue)
                {
                    urls.Add($"http://localhost:{apiSettings.HttpPort.Value}");
                }
                
                if (apiSettings.HttpsPort.HasValue)
                {
                    urls.Add($"https://localhost:{apiSettings.HttpsPort.Value}");
                }
            }

            // Apply URL configuration if any URLs were specified
            if (urls.Count > 0)
            {
                Builder.WebHost.UseUrls(urls.ToArray());
            }
        }
    }

    /// <summary>
    /// Register the message handler - override this method to provide custom message handling
    /// </summary>
    protected virtual void RegisterMessageHandler()
    {
        Builder.Services.AddScoped<IMessageHandler, DefaultMessageHandler>();
    }

    /// <summary>
    /// Configure additional services - override this method to add custom services
    /// </summary>
    protected virtual void ConfigureServices()
    {
        // Override in derived classes to add custom services
    }

    /// <summary>
    /// Configure the application pipeline - override this method to add custom middleware
    /// </summary>
    protected virtual void ConfigureApplication(WebApplication app)
    {
        // Override in derived classes to add custom middleware
    }

    /// <summary>
    /// Build and configure the application
    /// </summary>
    protected virtual WebApplication BuildApplication()
    {
        // Allow derived classes to configure additional services
        ConfigureServices();

        App = Builder.Build();

        // Get API settings
        var apiSettings = App.Configuration.GetSection(ApiSettings.SectionName).Get<ApiSettings>() ?? new ApiSettings();

        // Configure base middleware
        if (App.Environment.IsDevelopment() || apiSettings.EnableSwaggerInProduction)
        {
            App.UseSwagger();
            App.UseSwaggerUI();
        }

        if (apiSettings.EnableHttpsRedirection)
        {
            App.UseHttpsRedirection();
        }
        
        App.UseCors();
        App.UseAuthorization();
        App.MapControllers();
        App.MapHealthChecks("/health");

        // Allow derived classes to configure additional middleware
        ConfigureApplication(App);

        return App;
    }

    /// <summary>
    /// Run the service
    /// </summary>
    public async Task RunAsync()
    {
        App = BuildApplication();
        await App.RunAsync();
    }

    /// <summary>
    /// Get a service from the dependency injection container
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        if (App?.Services == null)
            throw new InvalidOperationException("Application not built yet. Call BuildApplication first.");
        
        return App.Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Configure database services - override this method to configure your specific DbContext
    /// </summary>
    protected virtual void ConfigureDatabase()
    {
        // Default implementation - derived classes should override this
        // to configure their specific DbContext
        
        // Example of how to configure a DbContext:
        // Builder.Services.AddDbContext<YourDbContext>(options =>
        //     options.UseNpgsql(Builder.Configuration.GetConnectionString("DefaultConnection")));
        
        // Register Unit of Work pattern (will be configured when DbContext is registered)
        // Builder.Services.AddScoped<IUnitOfWork, UnitOfWork<YourDbContext>>();
    }

    /// <summary>
    /// Helper method to configure PostgreSQL DbContext with standard settings
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="connectionStringName">Connection string name (default: "DefaultConnection")</param>
    protected void ConfigurePostgreSqlDbContext<TContext>(string connectionStringName = "DefaultConnection") 
        where TContext : BaseDbContext
    {
        Builder.Services.AddDbContext<TContext>(options =>
        {
            var connectionString = Builder.Configuration.GetConnectionString(connectionStringName);
            var databaseSettings = Builder.Configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();
            
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: databaseSettings.MaxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(databaseSettings.MaxRetryDelay),
                    errorCodesToAdd: null);
            });

            options.EnableSensitiveDataLogging(databaseSettings.EnableSensitiveDataLogging);
            options.EnableDetailedErrors(databaseSettings.EnableDetailedErrors);
        });

        // Register Unit of Work
        Builder.Services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
    }

    /// <summary>
    /// Configure the API to listen on a specific HTTP port
    /// </summary>
    /// <param name="port">The HTTP port number</param>
    protected void ConfigureHttpPort(int port)
    {
        Builder.WebHost.UseUrls($"http://localhost:{port}");
    }

    /// <summary>
    /// Configure the API to listen on specific HTTP and HTTPS ports
    /// </summary>
    /// <param name="httpPort">The HTTP port number</param>
    /// <param name="httpsPort">The HTTPS port number</param>
    protected void ConfigureHttpPorts(int httpPort, int httpsPort)
    {
        Builder.WebHost.UseUrls($"http://localhost:{httpPort}", $"https://localhost:{httpsPort}");
    }

    /// <summary>
    /// Configure the API to listen on custom URLs
    /// </summary>
    /// <param name="urls">Array of URLs to listen on</param>
    protected void ConfigureUrls(params string[] urls)
    {
        Builder.WebHost.UseUrls(urls);
    }
}
