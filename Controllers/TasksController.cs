using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models.DTOs;

namespace ToDoApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController(ITaskService service, IValidator<CreateTaskRequest> createValidator, IValidator<UpdateTaskRequest> updateValidator) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetTasks()
  {
    IReadOnlyList<TaskResponse> response = await service.GetAll();
    return Ok(response);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<TaskResponse>> GetTaskById(Guid id)
  {
    TaskResponse response = await service.GetById(id);
    return Ok(response);
  }

  [HttpPost]
  public async Task<ActionResult<TaskResponse>> CreateTask(CreateTaskRequest request)
  {
    await createValidator.ValidateAndThrowAsync(request);

    TaskResponse createdTask = await service.Create(request);
    return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> UpdateTask(Guid id, UpdateTaskRequest request)
  {
    await updateValidator.ValidateAndThrowAsync(request);

    await service.Update(id, request);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> DeleteTask(Guid id)
  {
    await service.Delete(id);
    return NoContent();
  }
}