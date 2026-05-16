using Microsoft.AspNetCore.Authorization;

namespace Todo.Api.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AdminOnlyAttribute : AuthorizeAttribute
{
  public AdminOnlyAttribute()
  {
    Policy = "AdminOnly";
  }
}