namespace ToDoApi.Models.DTOs;

public sealed record RefreshTokenRequest(
  Guid UserId,
  string RefreshToken
);