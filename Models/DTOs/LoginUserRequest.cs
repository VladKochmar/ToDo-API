namespace ToDoApi.Models.DTOs;

public sealed record LoginUserRequest(
  string Email,
  string Password
);