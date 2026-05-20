using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Todo.Api.Tests.Auth;

namespace Todo.Api.Tests.Factories;

public class CustomWebApplicationFactory<TProgram> 
  : WebApplicationFactory<TProgram> where TProgram : class
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.ConfigureTestServices(services =>
    {
      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = "TestScheme";
        options.DefaultChallengeScheme = "TestScheme";
      })
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => {});
    });
    base.ConfigureWebHost(builder);
  }
}