using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Todo.Api.Tests.Auth;

public class TestAuthHandler
(
  IOptionsMonitor<AuthenticationSchemeOptions> options, 
  ILoggerFactory logger, 
  UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    if (!Request.Headers.TryGetValue("x-test-authId", out var authId))
    {
      authId = $"auth0|test-{Guid.NewGuid()}";
    }

    List<Claim> claims = 
    [
      new Claim(ClaimTypes.NameIdentifier, authId.ToString()),
      new Claim("client_id", "test-user-id")
    ];

    ClaimsIdentity identify = new(claims, "Test");
    ClaimsPrincipal principal = new(identify);
    AuthenticationTicket ticket = new (principal, "TestScheme");

    AuthenticateResult result = AuthenticateResult.Success(ticket);

    return  Task.FromResult(result);
  }
}