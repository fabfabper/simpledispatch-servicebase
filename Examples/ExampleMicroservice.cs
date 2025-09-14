using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using SimpleDispatch.ServiceBase;
using SimpleDispatch.ServiceBase.Database;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using SimpleDispatch.ServiceBase.Interfaces;

namespace SimpleDispatch.ServiceBase.Examples;

/// <summary>
/// Example implementation showing how to create a microservice using the base service
/// </summary>
public class ExampleMicroservice : BaseService
{
    public ExampleMicroservice(string[] args) : base(args)
    {
    }

    protected override void RegisterMessageHandler()
    {
        // Register a custom message handler instead of the default one
        Builder.Services.AddScoped<IMessageHandler, ExampleMessageHandler>();
    }

    protected override void ConfigureDatabase()
    {
        // Configure PostgreSQL database with the example DbContext
        ConfigurePostgreSqlDbContext<ExampleDbContext>();
    }

    protected override void ConfigureServices()
    {
        // Add any additional services specific to this microservice
        Builder.Services.AddScoped<IExampleService, ExampleService>();
        Builder.Services.AddScoped<IExampleRepository, ExampleRepository>();
    }
}

/// <summary>
/// Example custom message handler
/// </summary>
public class ExampleMessageHandler : IMessageHandler
{
    private readonly ILogger<ExampleMessageHandler> _logger;
    private readonly IExampleService _exampleService;

    public ExampleMessageHandler(
        ILogger<ExampleMessageHandler> logger,
        IExampleService exampleService)
    {
        _logger = logger;
        _exampleService = exampleService;
    }

    public async Task HandleMessageAsync(string message, BasicDeliverEventArgs args)
    {
        _logger.LogInformation("Processing message in ExampleMessageHandler: {Message}", message);

        try
        {
            // Implement your custom message processing logic here
            await _exampleService.ProcessMessageAsync(message);
            
            _logger.LogInformation("Message processed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Message}", message);
            throw; // Re-throw to trigger message requeue
        }
    }
}

/// <summary>
/// Example service interface
/// </summary>
public interface IExampleService
{
    Task ProcessMessageAsync(string message);
}

/// <summary>
/// Example service implementation
/// </summary>
public class ExampleService : IExampleService
{
    private readonly ILogger<ExampleService> _logger;
    private readonly IExampleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ExampleService(
        ILogger<ExampleService> logger,
        IExampleRepository repository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task ProcessMessageAsync(string message)
    {
        _logger.LogInformation("Processing message in ExampleService: {Message}", message);
        
        try
        {
            // Start a transaction
            await _unitOfWork.BeginTransactionAsync();

            // Create a new entity from the message
            var entity = new ExampleEntity
            {
                Name = $"Message_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Description = message,
                CreatedAt = DateTime.UtcNow
            };

            // Save to database
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            // Commit the transaction
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Message processing completed and saved to database with ID: {EntityId}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message, rolling back transaction");
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}

/// <summary>
/// Example DbContext for demonstration
/// </summary>
public class ExampleDbContext : BaseDbContext
{
    public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options)
    {
    }

    public DbSet<ExampleEntity> ExampleEntities { get; set; } = null!;

    protected override void ConfigureEntities(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExampleEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}

/// <summary>
/// Example entity for demonstration
/// </summary>
public class ExampleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Example repository interface
/// </summary>
public interface IExampleRepository : IRepository<ExampleEntity, int>
{
    Task<IEnumerable<ExampleEntity>> GetByNameAsync(string name);
    Task<IEnumerable<ExampleEntity>> GetRecentAsync(int count = 10);
}

/// <summary>
/// Example repository implementation
/// </summary>
public class ExampleRepository : BaseRepository<ExampleEntity, int, ExampleDbContext>, IExampleRepository
{
    public ExampleRepository(ExampleDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExampleEntity>> GetByNameAsync(string name)
    {
        return await FindAsync(e => e.Name.Contains(name));
    }

    public async Task<IEnumerable<ExampleEntity>> GetRecentAsync(int count = 10)
    {
        return await Context.ExampleEntities
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
}
