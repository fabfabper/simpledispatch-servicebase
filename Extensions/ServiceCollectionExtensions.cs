using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleDispatch.ServiceBase.Configuration;
using SimpleDispatch.ServiceBase.Database;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Services;

namespace SimpleDispatch.ServiceBase.Extensions;

/// <summary>
/// Extension methods for configuring SimpleDispatch base services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add SimpleDispatch base services to the DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureRabbitMq">Action to configure RabbitMQ settings</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSimpleDispatchBase(
        this IServiceCollection services,
        Action<RabbitMqSettings>? configureRabbitMq = null)
    {
        // Configure RabbitMQ if configuration action is provided
        if (configureRabbitMq != null)
        {
            services.Configure(configureRabbitMq);
        }

        // Register core services
        services.AddSingleton<IRabbitMqClient, RabbitMqClient>();
        services.AddScoped<IMessageHandler, DefaultMessageHandler>();
        services.AddHostedService<MessageConsumerService>();

        return services;
    }

    /// <summary>
    /// Add a custom message handler to the DI container
    /// </summary>
    /// <typeparam name="T">Message handler implementation type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMessageHandler<T>(this IServiceCollection services)
        where T : class, IMessageHandler
    {
        services.AddScoped<IMessageHandler, T>();
        return services;
    }

    /// <summary>
    /// Add PostgreSQL database support to the DI container
    /// </summary>
    /// <typeparam name="TContext">DbContext type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="connectionStringName">Connection string name (default: "DefaultConnection")</param>
    /// <param name="configureDatabaseSettings">Action to configure database settings</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddPostgreSqlDatabase<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "DefaultConnection",
        Action<DatabaseSettings>? configureDatabaseSettings = null)
        where TContext : BaseDbContext
    {
        // Configure database settings
        var databaseSettings = new DatabaseSettings();
        configuration.GetSection(DatabaseSettings.SectionName).Bind(databaseSettings);
        configureDatabaseSettings?.Invoke(databaseSettings);
        services.Configure<DatabaseSettings>(options =>
        {
            configuration.GetSection(DatabaseSettings.SectionName).Bind(options);
            configureDatabaseSettings?.Invoke(options);
        });

        // Configure DbContext
        services.AddDbContext<TContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);

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
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();

        return services;
    }

    /// <summary>
    /// Add a repository to the DI container
    /// </summary>
    /// <typeparam name="TRepository">Repository interface type</typeparam>
    /// <typeparam name="TImplementation">Repository implementation type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddRepository<TRepository, TImplementation>(this IServiceCollection services)
        where TRepository : class
        where TImplementation : class, TRepository
    {
        services.AddScoped<TRepository, TImplementation>();
        return services;
    }
}
