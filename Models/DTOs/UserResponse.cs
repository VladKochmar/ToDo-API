namespace ToDoApi.Models.DTOs;

public sealed record UserResponse(
  Guid Id,
  string FirstName,
  string LastName,
  string Email
);