using Microsoft.AspNetCore.Mvc;
using SimpleDispatch.ServiceBase.Interfaces;

namespace SimpleDispatch.ServiceBase.Controllers;

/// <summary>
/// Base API controller that provides common functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IRabbitMqClient RabbitMqClient;

    protected BaseApiController(IRabbitMqClient rabbitMqClient)
    {
        RabbitMqClient = rabbitMqClient;
    }

    /// <summary>
    /// Publish a message to RabbitMQ
    /// </summary>
    /// <param name="message">Message to publish</param>
    /// <param name="routingKey">Routing key for the message</param>
    /// <returns>Action result</returns>
    protected async Task<IActionResult> PublishMessageAsync(string message, string routingKey = "")
    {
        try
        {
            await RabbitMqClient.PublishMessageAsync(message, routingKey);
            return Ok(new { Message = "Message published successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}
