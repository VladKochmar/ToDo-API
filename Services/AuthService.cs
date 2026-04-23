using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ToDoApi.Data;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class AuthService(AppDbContext context, IConfiguration configuration)
{
  private async Task<User?> ValidateRefreshToken(Guid userId, string refreshToken)
  {
    User? user = await context.Users.FindAsync(userId);
    if (
      user is null || 
      user.RefreshToken != refreshToken || 
      user.RefreshTokenExpiryTime <= DateTime.UtcNow
    )
      return null;

    return user;
  }
  
  private string CreateAccessToken(User user)
  {
    List<Claim> claims =
    [
      new (ClaimTypes.Email, user.Email),
      new (ClaimTypes.NameIdentifier, user.Id.ToString())
    ];

    SymmetricSecurityKey key = new (Encoding.UTF8.GetBytes(configuration.GetValue<string>("JwtConfig:Key")!));

    SigningCredentials credentials = new (key, SecurityAlgorithms.HmacSha512);

    JwtSecurityToken tokenDescription = new (
      issuer: configuration.GetValue<string>("JwtConfig:Issuer"),
      audience: configuration.GetValue<string>("JwtConfig:Audience"),
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(15),
      signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(tokenDescription);
  }

  private string GenerateRefreshToken()
  {
    return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
  }

  private async Task<TokenResponse> CreateTokenResponse(User user)
  {
    return new TokenResponse(
      CreateAccessToken(user),
      await GenerateAndSaveRefreshToken(user)
    );
  }

  private async Task<string> GenerateAndSaveRefreshToken(User user)
  {
    string? refreshToken = GenerateRefreshToken();
    user.RefreshToken = refreshToken;
    user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);
    await context.SaveChangesAsync();
    return refreshToken;
  }

  public async Task<TokenResponse?> Login(LoginUserRequest request)
  {
    User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
    if (user is null)
      return null;
    
    if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
      == PasswordVerificationResult.Failed)
    {
      return null;
    }

    return await CreateTokenResponse(user);
  }

  public async Task<User?> Register(RegisterUserRequest request)
  {
    if (await context.Users.AnyAsync(u => u.Email == request.Email))
      return null;

    string? hashedPasswrod = new PasswordHasher<User>().HashPassword(null!, request.Password);

    User user = new ()
    {
      Id = Guid.CreateVersion7(),
      FirstName = request.FirstName,
      LastName = request.LastName,
      Email = request.Email,
      PasswordHash = hashedPasswrod
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();

    return user;
  }

  public async Task<TokenResponse?> RefreshTokens(RefreshTokenRequest request)
  {
    User? user = await ValidateRefreshToken(request.UserId, request.RefreshToken);
    if (user is null) return null;

    return await CreateTokenResponse(user);
  }
}