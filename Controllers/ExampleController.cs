using Microsoft.AspNetCore.Mvc;
using SimpleDispatch.ServiceBase.Controllers;
using SimpleDispatch.ServiceBase.Database.Interfaces;
using SimpleDispatch.ServiceBase.Examples;
using SimpleDispatch.ServiceBase.Interfaces;
using SimpleDispatch.ServiceBase.Models;

namespace SimpleDispatch.ServiceBase.Controllers;

/// <summary>
/// Example controller demonstrating database operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExampleController : BaseApiController
{
    private readonly IExampleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ExampleController(
        IRabbitMqClient rabbitMqClient,
        IExampleRepository repository,
        IUnitOfWork unitOfWork) : base(rabbitMqClient)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all example entities
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExampleEntity>>>> GetAll()
    {
        try
        {
            var entities = await _repository.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<ExampleEntity>>.CreateSuccess(entities));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<ExampleEntity>>.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ExampleEntity>>> GetById(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound(ApiResponse<ExampleEntity>.CreateError("Entity not found"));
            }

            return Ok(ApiResponse<ExampleEntity>.CreateSuccess(entity));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ExampleEntity>.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExampleEntity>>> Create([FromBody] CreateExampleEntityRequest request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var entity = new ExampleEntity
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            var createdEntity = await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            // Publish event
            await PublishMessageAsync($"Entity created: {createdEntity.Name}", "entity.created");

            await _unitOfWork.CommitTransactionAsync();

            return CreatedAtAction(nameof(GetById), new { id = createdEntity.Id },
                ApiResponse<ExampleEntity>.CreateSuccess(createdEntity, "Entity created successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return StatusCode(500, ApiResponse<ExampleEntity>.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Update an entity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ExampleEntity>>> Update(int id, [FromBody] UpdateExampleEntityRequest request)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound(ApiResponse<ExampleEntity>.CreateError("Entity not found"));
            }

            await _unitOfWork.BeginTransactionAsync();

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.UpdatedAt = DateTime.UtcNow;

            var updatedEntity = await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            return Ok(ApiResponse<ExampleEntity>.CreateSuccess(updatedEntity, "Entity updated successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return StatusCode(500, ApiResponse<ExampleEntity>.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Delete an entity
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound(ApiResponse<object>.CreateError("Entity not found"));
            }

            await _unitOfWork.BeginTransactionAsync();

            await _repository.DeleteAsync(entity);
            await _repository.SaveChangesAsync();

            await _unitOfWork.CommitTransactionAsync();

            return Ok(ApiResponse<object>.CreateSuccess(new { }, "Entity deleted successfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return StatusCode(500, ApiResponse<object>.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Search entities by name
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExampleEntity>>>> Search([FromQuery] string name)
    {
        try
        {
            var entities = await _repository.GetByNameAsync(name);
            return Ok(ApiResponse<IEnumerable<ExampleEntity>>.CreateSuccess(entities));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<ExampleEntity>>.CreateError(ex.Message));
        }
    }

    /// <summary>
    /// Get recent entities
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExampleEntity>>>> GetRecent([FromQuery] int count = 10)
    {
        try
        {
            var entities = await _repository.GetRecentAsync(count);
            return Ok(ApiResponse<IEnumerable<ExampleEntity>>.CreateSuccess(entities));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<IEnumerable<ExampleEntity>>.CreateError(ex.Message));
        }
    }
}

/// <summary>
/// Request model for creating an example entity
/// </summary>
public class CreateExampleEntityRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Request model for updating an example entity
/// </summary>
public class UpdateExampleEntityRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
