using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Exceptions;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class TaskService(AppDbContext context) : ITaskService
{
  public async Task<TaskResponse> Create(Guid userId, CreateTaskRequest request)
  {
    await VerifyCategoryById(request.CategoryId, userId);

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

    return await GetById(newTask.Id, userId);
  }

  public async Task Delete(Guid id, Guid userId)
  {
    TaskItem taskItem = await GetTaskOrThrow(id, userId);

    context.Tasks.Remove(taskItem);
    await context.SaveChangesAsync();
  }

  public async Task<IReadOnlyList<TaskResponse>> GetAll(Guid userId)
  {
    List<TaskResponse> tasks = await context.Tasks
      .Where(t => t.UserId == userId)
      .Select(t => new TaskResponse(
        t.Id,
        t.Title,
        t.IsCompleted,
        t.Description,
        t.Category != null ? t.Category.Title : null,
        t.Category != null ? t.Category.Id : null,
        t.DueDate
      ))
      .ToListAsync();

    return tasks;
  }

  public async Task<IReadOnlyList<TaskResponse>> GetAllByCategory(Guid categoryId, Guid userId)
  {
    await VerifyCategoryById(categoryId, userId);
    List<TaskResponse> tasks = await context.Tasks
      .Where(t => t.UserId == userId && t.CategoryId == categoryId)
      .Select(t => new TaskResponse(
        t.Id,
        t.Title,
        t.IsCompleted,
        t.Description,
        t.Category != null ? t.Category.Title : null,
        t.Category != null ? t.Category.Id : null,
        t.DueDate
      ))
      .ToListAsync();

    return tasks;
  }

  public async Task<TaskResponse> GetById(Guid id, Guid userId)
  {
    TaskItem taskItem = await GetTaskOrThrow(id, userId);

    return new TaskResponse(
      taskItem.Id,
      taskItem.Title,
      taskItem.IsCompleted,
      taskItem.Description,
      taskItem.Category?.Title,
      taskItem.Category?.Id,
      taskItem.DueDate
    );
  }

  public async Task Update(Guid id, Guid userId, UpdateTaskRequest request)
  {
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
      await VerifyCategoryById(request.CategoryId, userId);
      taskItem.CategoryId = request.CategoryId;
    }

    await context.SaveChangesAsync();
  }

  private async Task VerifyCategoryById(Guid? categoryId, Guid userId)
  {
    if (!categoryId.HasValue) return;

    bool exists = await context.Categories.AnyAsync(c => c.Id == categoryId.Value && c.UserId == userId);
    if (!exists) throw new NotFoundException(categoryId.Value, "Category");
  }

  private async Task<TaskItem> GetTaskOrThrow(Guid taskId, Guid userId)
  {
    TaskItem? taskItem = await context.Tasks
      .Include(t => t.Category)
      .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
    
    if (taskItem is null)
      throw new NotFoundException(taskId, "Task");

    return taskItem;
  }
}