using Microsoft.AspNetCore.Mvc;
using SimpleDispatch.ServiceBase.Controllers;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Models;

namespace SimpleDispatch.ServiceBase.Controllers;

/// <summary>
/// Sample messaging controller that demonstrates the base functionality
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MessagingController : BaseApiController
{
    public MessagingController(IRabbitMqClient rabbitMqClient) : base(rabbitMqClient)
    {
    }

    /// <summary>
    /// Publish a message to RabbitMQ
    /// </summary>
    /// <param name="request">Message publish request</param>
    /// <returns>API response</returns>
    [HttpPost("publish")]
    public async Task<ActionResult<ApiResponse<object>>> PublishMessage([FromBody] PublishMessageRequest request)
    {
        try
        {
            await RabbitMqClient.PublishMessageAsync(request.Message, request.RoutingKey);
            
            return Ok(ApiResponse<object>.CreateSuccess(
                new { MessageId = Guid.NewGuid().ToString() }, 
                "Message published successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    public ActionResult<ApiResponse<object>> Health()
    {
        return Ok(ApiResponse<object>.CreateSuccess(
            new { Status = "Healthy", Service = "SimpleDispatch Base Service" }));
    }
}
