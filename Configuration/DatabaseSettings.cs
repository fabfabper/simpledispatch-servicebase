namespace SimpleDispatch.ServiceBase.Configuration;

/// <summary>
/// Configuration settings for PostgreSQL database connection
/// </summary>
public class DatabaseSettings
{
    public const string SectionName = "Database";

    /// <summary>
    /// PostgreSQL connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Maximum number of retry attempts for database operations
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Maximum delay between retry attempts (in seconds)
    /// </summary>
    public int MaxRetryDelay { get; set; } = 30;

    /// <summary>
    /// Whether to enable sensitive data logging (should be false in production)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Whether to enable detailed errors (should be false in production)
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
}
