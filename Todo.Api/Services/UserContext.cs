using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;

namespace Todo.Api.Services;

public class UserContext(IHttpContextAccessor accessor, AppDbContext context) : IUserContext
{
  private string AuthId()
  {
    string? authId = accessor.HttpContext?
      .User
      .FindFirst(ClaimTypes.NameIdentifier)?
      .Value;

    if (authId is null)
      throw new UnauthorizedAccessException("Missing sub claim.");

    return authId;
  }

  public async Task<Guid> UserId()
  {
    string authId = AuthId();
    Guid userId = await context.Users
      .Where(u => u.AuthId == authId)
      .Select(u => u.Id)
      .FirstOrDefaultAsync();

    if (userId == Guid.Empty)
      throw new InvalidOperationException("Authenticated user does not exist in the database.");

    return userId;
  }
}