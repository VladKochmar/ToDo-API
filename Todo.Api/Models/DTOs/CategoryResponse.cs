namespace Todo.Api.Models.DTOs;

public sealed record CategoryResponse(
  Guid Id,
  string Title
);