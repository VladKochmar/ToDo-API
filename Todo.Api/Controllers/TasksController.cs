using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Models.DTOs;
using Todo.Api.Services;

namespace Todo.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1")]
public class TasksController(
  ITaskService service, 
  IUserContext userContext,
  IValidator<CreateTaskRequest> createValidator, 
  IValidator<UpdateTaskRequest> updateValidator
) : ControllerBase
{
  [HttpGet("tasks")]
  public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasks()
  {
    Guid userId = await userContext.UserId();
    IReadOnlyList<TaskResponse> response = await service.GetAll(userId);
    return Ok(response);
  }

  [HttpGet("categories/{id:guid}/tasks")]
  public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasksByCategory(Guid id)
  {
    Guid userId = await userContext.UserId();
    IReadOnlyList<TaskResponse> response = await service.GetAllByCategory(id, userId);
    return Ok(response);
  }

  [HttpGet("tasks/{id:guid}")]
  public async Task<ActionResult<TaskResponse>> GetTaskById(Guid id)
  {
    Guid userId = await userContext.UserId();
    TaskResponse response = await service.GetById(id, userId);
    return Ok(response);
  }

  [HttpPost("tasks")]
  public async Task<ActionResult<TaskResponse>> CreateTask(CreateTaskRequest request)
  {
    Guid userId = await userContext.UserId();
    await createValidator.ValidateAndThrowAsync(request);

    TaskResponse createdTask = await service.Create(userId, request);
    return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
  }

  [HttpPut("tasks/{id:guid}")]
  public async Task<ActionResult> UpdateTask(Guid id, UpdateTaskRequest request)
  {
    Guid userId = await userContext.UserId();
    await updateValidator.ValidateAndThrowAsync(request);

    await service.Update(id, userId, request);
    return NoContent();
  }

  [HttpDelete("tasks/{id:guid}")]
  public async Task<ActionResult> DeleteTask(Guid id)
  {
    Guid userId = await userContext.UserId();
    await service.Delete(id, userId);
    return NoContent();
  }
}