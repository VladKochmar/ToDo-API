namespace ToDoApi.Models.DTOs;

public sealed record TokenResponse(
  string AccessToken,
  string RefreshToken
);