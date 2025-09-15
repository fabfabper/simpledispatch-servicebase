namespace SimpleDispatch.ServiceBase.Configuration;

/// <summary>
/// Configuration settings for the API server
/// </summary>
public class ApiSettings
{
    public const string SectionName = "Api";

    /// <summary>
    /// The port number for the HTTP server
    /// </summary>
    public int? HttpPort { get; set; }

    /// <summary>
    /// The port number for the HTTPS server
    /// </summary>
    public int? HttpsPort { get; set; }

    /// <summary>
    /// The URLs to listen on (overrides ports if specified)
    /// Example: ["http://localhost:5000", "https://localhost:5001"]
    /// </summary>
    public string[]? Urls { get; set; }

    /// <summary>
    /// Whether to enable HTTPS redirection
    /// </summary>
    public bool EnableHttpsRedirection { get; set; } = true;

    /// <summary>
    /// Whether to enable Swagger in production
    /// </summary>
    public bool EnableSwaggerInProduction { get; set; } = false;
}
