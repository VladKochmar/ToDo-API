namespace ToDoApi.Models.DTOs;

public sealed record CreateUserRequest(
  string Email,
  string FullName
);