using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Todo.Api.Authorization;

public class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
{
  protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyRequirement requirement)
  {
    if (context.User.Identity?.IsAuthenticated != true)
    {
      return Task.CompletedTask;
    }

    string? adminSub = requirement.AdminSub;
    string? userSub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (!string.IsNullOrEmpty(adminSub) && userSub == adminSub)
    {
      context.Succeed(requirement);
    }

    return Task.CompletedTask;
  }
}