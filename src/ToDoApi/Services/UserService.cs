using System.IdentityModel.Tokens.Jwt;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class UserService(AppDbContext context, IValidator<CreateUserRequest> validator) : IUserService
{
  public async Task SyncUser(string idToken)
  {
    JwtSecurityTokenHandler handler = new();
    if (!handler.CanReadToken(idToken))
      throw new ArgumentException("Invalid token format.");

    JwtSecurityToken token = handler.ReadJwtToken(idToken);

    string sub = token.Subject;
    CreateUserRequest request = new (
      token.Claims.FirstOrDefault(c => c.Type == "email")?.Value ?? string.Empty,
      token.Claims.FirstOrDefault(c => c.Type == "name")?.Value ?? string.Empty
    );

    await validator.ValidateAndThrowAsync(request);

    User? user = await context.Users.FirstOrDefaultAsync(u => u.AuthId == sub);

    if (user is null)
    {
      context.Users.Add(new User
      {
        Id = Guid.CreateVersion7(),
        AuthId = sub,
        Email = request.Email,
        FullName = request.FullName,
      });
    } 
    else
    {
      user.Email = request.Email;
      user.FullName = request.FullName;
    }

    await context.SaveChangesAsync();
  }
}