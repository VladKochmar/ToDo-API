namespace ToDoApi.Models.DTOs;

public sealed record UpdateTaskRequest(
  Guid Id,
  string Title,
  bool IsCompleted,
  string? Description,
  DateTimeOffset? DueDate,
  Guid? CategoryId
);