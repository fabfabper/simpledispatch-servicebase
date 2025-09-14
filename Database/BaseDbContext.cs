using Microsoft.EntityFrameworkCore;

namespace SimpleDispatch.ServiceBase.Database;

/// <summary>
/// Base DbContext for SimpleDispatch microservices
/// </summary>
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <summary>
    /// Override this method to configure your entity models
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure common entity configurations here if needed
        ConfigureCommonEntities(modelBuilder);
        
        // Allow derived classes to add their own configurations
        ConfigureEntities(modelBuilder);
    }

    /// <summary>
    /// Configure common entities that all microservices might use
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected virtual void ConfigureCommonEntities(ModelBuilder modelBuilder)
    {
        // Add common entity configurations here
        // For example: audit fields, soft delete configurations, etc.
    }

    /// <summary>
    /// Override this method in derived classes to configure specific entities
    /// </summary>
    /// <param name="modelBuilder">Model builder instance</param>
    protected abstract void ConfigureEntities(ModelBuilder modelBuilder);
}
