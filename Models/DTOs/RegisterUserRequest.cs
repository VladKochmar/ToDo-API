namespace ToDoApi.Models.DTOs;

public sealed record RegisterUserRequest(
  string Email,
  string FirstName,
  string LastName,
  string Password
);