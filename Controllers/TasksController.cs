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
  public async Task<IActionResult> GetTaskById(Guid id) =>
    Ok(await service.GetById(id));

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

    await service.Update(id, request);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteTask(Guid id)
  {
    await service.Delete(id);
    return NoContent();
  }
}