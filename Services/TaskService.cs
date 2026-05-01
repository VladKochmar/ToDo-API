using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Exceptions;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class TaskService(AppDbContext context, IUserContext userContext) : ITaskService
{
  public async Task<TaskResponse> Create(CreateTaskRequest request)
  {
    Guid userId = await GetCurrentUserId();

    if (request.CategoryId is not null)
    {
      bool categoryExists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);
      if (!categoryExists) throw new NotFoundException(request.CategoryId.Value, "Category");
    }

    bool taskExists = await context.Tasks.AnyAsync(t => t.Title == request.Title && 
      t.CategoryId == request.CategoryId && t.UserId == userId);

    if (taskExists)
      throw new ConflictException($"Task '{request.Title}' already exists.");

    TaskItem newTask = new ()
    {
      Id = Guid.CreateVersion7(),
      Title = request.Title.Trim(),
      Description = request.Description?.Trim(),
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

  public async Task Delete(Guid id)
  {
    Guid userId = await GetCurrentUserId();
    TaskItem taskItem = await GetTaskOrThrow(id, userId);

    context.Tasks.Remove(taskItem);
    await context.SaveChangesAsync();
  }

  public async Task<IReadOnlyList<TaskResponse>> GetAll()
  {
    Guid userId = await GetCurrentUserId();

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

  public async Task<TaskResponse> GetById(Guid id)
  {
    Guid userId = await GetCurrentUserId();
    TaskItem taskItem = await GetTaskOrThrow(id, userId);

    return new TaskResponse(
      taskItem.Id,
      taskItem.Title,
      taskItem.IsCompleted,
      taskItem.Description,
      taskItem.Category?.Title,
      taskItem.DueDate
    );
  }

  public async Task Update(Guid id, UpdateTaskRequest request)
  {
    Guid userId = await GetCurrentUserId();
    TaskItem taskItem = await GetTaskOrThrow(id, userId);

    if (taskItem.IsCompleted)
      throw new ConflictException("Task already completed.");

    bool taskExists = await context.Tasks.AnyAsync(t => t.Title == request.Title &&
    t.CategoryId == request.CategoryId && t.UserId == userId && t.Id != taskItem.Id);

    if (taskExists)
      throw new ConflictException($"Task '{request.Title}' already exists.");

    taskItem.Title = request.Title.Trim();
    taskItem.DueDate = request.DueDate;
    taskItem.IsCompleted = request.IsCompleted;
    taskItem.Description = request.Description?.Trim();

    if (taskItem.CategoryId != request.CategoryId)
    {
      if (request.CategoryId.HasValue)
      {
        bool exists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.UserId == userId);
        if (!exists) throw new NotFoundException(request.CategoryId.Value, "Category");
      }
      taskItem.CategoryId = request.CategoryId;
    }

    await context.SaveChangesAsync();
  }

  private async Task<Guid> GetCurrentUserId()
  {
    string authId = userContext.AuthId();
    Guid userId = await context.Users
      .Where(u => u.AuthId == authId)
      .Select(u => u.Id)
      .FirstOrDefaultAsync();

    return userId;
  }

  private async Task<TaskItem> GetTaskOrThrow(Guid taskId, Guid userId)
  {
    TaskItem? taskItem = await context.Tasks
      .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
    
    if (taskItem is null)
      throw new NotFoundException(taskId, "Task");

    return taskItem;
  }
}