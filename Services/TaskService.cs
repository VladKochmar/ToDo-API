using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class TaskService(AppDbContext context, ICurrentUserService userService) : ITaskService
{
  public async Task<TaskResponse> Create(CreateTaskRequest request)
  {
    Guid userId = userService.GetUserId();

    if (request.CategoryId is not null)
    {
      bool exists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);
      if (!exists) throw new ArgumentException($"Category with the given ID not found.");
    }

    TaskItem newTask = new ()
    {
      Id = Guid.CreateVersion7(),
      Title = request.Title,
      Description = request.Description,
      DueDate = request.DueDate,
      CategoryId = request.CategoryId,
      UserId = userId,
      CreatedAt = DateTimeOffset.UtcNow
    };

    context.Tasks.Add(newTask);
    await context.SaveChangesAsync();

    return new TaskResponse(
      newTask.Id,
      newTask.Title,
      newTask.IsCompleted,
      newTask.Description,
      newTask.Category?.Title,
      newTask.DueDate
    );
  }

  public async Task<bool> Delete(Guid id)
  {
    Guid userId = userService.GetUserId();

    TaskItem? taskItem = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    if (taskItem is null) return false;

    context.Tasks.Remove(taskItem);
    await context.SaveChangesAsync();

    return true;
  }

  public async Task<IReadOnlyList<TaskResponse>> GetAll()
  {
    Guid userId = userService.GetUserId();

    List<TaskResponse> tasks = await context.Tasks
      .Where(t => t.UserId == userId)
      .Select(t => new TaskResponse(
        t.Id,
        t.Title,
        t.IsCompleted,
        t.Description,
        t.Category != null ? t.Category.Title : null,
        t.DueDate
      ))
      .ToListAsync();

    return tasks;
  }

  public async Task<TaskResponse?> GetById(Guid id)
  {
    Guid userId = userService.GetUserId();

    TaskItem? taskItem = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    if (taskItem is null) return null;

    return new TaskResponse(
      taskItem.Id,
      taskItem.Title,
      taskItem.IsCompleted,
      taskItem.Description,
      taskItem.Category?.Title,
      taskItem.DueDate
    );
  }

  public async Task<bool> Update(Guid id, UpdateTaskRequest request)
  {
    Guid userId = userService.GetUserId();

    TaskItem? taskItem = await context.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    
    if (taskItem is null) 
      return false;

    if (taskItem.IsCompleted)
      return false;

    taskItem.Title = request.Title;
    taskItem.DueDate = request.DueDate;
    taskItem.IsCompleted = request.IsCompleted;
    taskItem.Description = request.Description;

    if (taskItem.CategoryId != request.CategoryId)
    {
      if (request.CategoryId.HasValue)
      {
        bool exists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);
        if (!exists) throw new ArgumentException($"Category with the given ID not found.");
      }
      taskItem.CategoryId = request.CategoryId;
    }

    await context.SaveChangesAsync();
    return true;
  }
}