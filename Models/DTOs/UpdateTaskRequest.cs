namespace ToDoApi.Models.DTOs;

public sealed record UpdateTaskRequest(
  string Title,
  bool IsCompleted,
  string? Description,
  DateTimeOffset? DueDate,
  Guid? CategoryId
);