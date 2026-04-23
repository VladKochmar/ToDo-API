using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;
using ToDoApi.Services;

namespace ToDoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, IValidator<LoginUserRequest> loginValidator, IValidator<RegisterUserRequest> registerValidator) : ControllerBase
{
  [HttpPost("register")]
  public async Task<ActionResult<User>> Register(RegisterUserRequest request)
  {
    await registerValidator.ValidateAndThrowAsync(request);

    User? user = await authService.Register(request);
    if (user is null) 
      return BadRequest("User already exists.");
    
    return Ok(user);
  }

  [HttpPost("login")]
  public async Task<ActionResult<TokenResponse>> Login(LoginUserRequest request)
  {
    await loginValidator.ValidateAndThrowAsync(request);

    TokenResponse? response = await authService.Login(request);
    if (response is null)
      return BadRequest("Invalid email or password.");

    return Ok(response);
  }

  [HttpPost("refresh-token")]
  public async Task<ActionResult<TokenResponse>> RefreshToken(RefreshTokenRequest request)
  {
    TokenResponse? result = await authService.RefreshTokens(request);
    if (
      result is null || 
      result.AccessToken is null ||
      result.RefreshToken is null
    )
      return Unauthorized("Invalid token.");

    return Ok(result);
  }
}