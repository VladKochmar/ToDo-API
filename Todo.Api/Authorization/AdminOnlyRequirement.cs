using Microsoft.AspNetCore.Authorization;

namespace Todo.Api.Authorization;

public class AdminOnlyRequirement(string? adminSub) : IAuthorizationRequirement
{
  public string? AdminSub { get; } = adminSub;
}