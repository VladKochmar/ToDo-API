namespace ToDoApi.Models.DTOs;

public sealed record TaskResponse(
  Guid Id,
  string Title,
  bool IsCompleted,
  string? Description,
  string? CategoryName,
  DateTimeOffset? DueDate
);