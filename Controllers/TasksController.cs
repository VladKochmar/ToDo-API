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
  public async Task<IActionResult> GetTasks() => Ok(await service.GetAll());

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetTaskById(Guid id)
  {
    TaskResponse? taskItem = await service.GetById(id);
    if (taskItem is null) 
      return NotFound("Task with the given ID was not found.");

    return Ok(taskItem);
  }

  [HttpPost]
  public async Task<IActionResult> CreateTask(CreateTaskRequest request)
  {
    await createValidator.ValidateAndThrowAsync(request);

    TaskResponse createdTask = await service.Create(request);
    return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> UpdateTask(Guid id, UpdateTaskRequest request)
  {
    await updateValidator.ValidateAndThrowAsync(request);

    bool updated = await service.Update(id, request);
    return updated ? NoContent() : NotFound("Task with the given ID was not found.");
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteTask(Guid id)
  {
    bool deleted = await service.Delete(id);
    return deleted ? NoContent() : NotFound("Task with the given ID was not found.");
  }
}