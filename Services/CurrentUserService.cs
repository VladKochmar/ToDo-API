using System.Security.Claims;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
  public Guid GetUserId()
  {
    string? userId = httpContextAccessor.HttpContext
      ?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (userId is null)
      throw new UnauthorizedAccessException();

    return Guid.Parse(userId);
  }
}