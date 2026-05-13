namespace Todo.Api.Models.DTOs;

public sealed record TaskResponse(
  Guid Id,
  string Title,
  bool IsCompleted,
  string? Description,
  string? CategoryName,
  Guid? CategoryId,
  DateTimeOffset? DueDate
);