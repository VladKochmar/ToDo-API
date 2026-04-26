namespace ToDoApi.Models.DTOs;

public sealed record CreateUserRequest(
  string Email,
  string FirstName,
  string LastName
);