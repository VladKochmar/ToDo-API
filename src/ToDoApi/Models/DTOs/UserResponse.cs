namespace ToDoApi.Models.DTOs;

public sealed record UserResponse(
  Guid Id,
  string FullName,
  string Email
);