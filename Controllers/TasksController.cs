using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models.DTOs;
using ToDoApi.Services;

namespace ToDoApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController(
  ITaskService service, 
  IUserContext userContext,
  IValidator<CreateTaskRequest> createValidator, 
  IValidator<UpdateTaskRequest> updateValidator
) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasks()
  {
    Guid userId = await userContext.UserId();
    IReadOnlyList<TaskResponse> response = await service.GetAll(userId);
    return Ok(response);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<TaskResponse>> GetTaskById(Guid id)
  {
    Guid userId = await userContext.UserId();
    TaskResponse response = await service.GetById(id, userId);
    return Ok(response);
  }

  [HttpPost]
  public async Task<ActionResult<TaskResponse>> CreateTask(CreateTaskRequest request)
  {
    Guid userId = await userContext.UserId();
    await createValidator.ValidateAndThrowAsync(request);

    TaskResponse createdTask = await service.Create(userId, request);
    return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> UpdateTask(Guid id, UpdateTaskRequest request)
  {
    Guid userId = await userContext.UserId();
    await updateValidator.ValidateAndThrowAsync(request);

    await service.Update(id, userId, request);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> DeleteTask(Guid id)
  {
    Guid userId = await userContext.UserId();
    await service.Delete(id, userId);
    return NoContent();
  }
}