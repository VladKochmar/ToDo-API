namespace ToDoApi.Models.DTOs;

public sealed record CreateTaskRequest(
  string Title,
  string? Description,
  DateTimeOffset? DueDate,
  Guid? CategoryId
);