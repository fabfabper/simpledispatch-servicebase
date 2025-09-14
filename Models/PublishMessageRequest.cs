using System.ComponentModel.DataAnnotations;

namespace SimpleDispatch.ServiceBase.Models;

/// <summary>
/// Model for publishing messages through the API
/// </summary>
public class PublishMessageRequest
{
    /// <summary>
    /// Message content to publish
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional routing key for the message
    /// </summary>
    public string RoutingKey { get; set; } = string.Empty;
}
