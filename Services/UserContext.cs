using System.Security.Claims;

namespace ToDoApi.Services;

public class UserContext(IHttpContextAccessor accessor) : IUserContext
{
  public string AuthId()
  {
    string? authId = accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (authId is null)
      throw new UnauthorizedAccessException();

    return authId;
  }
}