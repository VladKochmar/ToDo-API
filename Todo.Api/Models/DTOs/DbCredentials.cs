namespace Todo.Api.Models.DTOs;

public sealed record DbCredentials(
  string DbName,
  string DbUser,
  string DbPassword
);