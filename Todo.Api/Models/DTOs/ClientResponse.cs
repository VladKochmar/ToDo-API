namespace Todo.Api.Models.DTOs;

public sealed record ClientResponse(
  Guid Id,
  string Name
);