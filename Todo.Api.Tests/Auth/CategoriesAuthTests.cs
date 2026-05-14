using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Todo.Api.Tests.Auth;

public class CategoriesAuthTests(WebApplicationFactory<Program> factory) 
  : IClassFixture<WebApplicationFactory<Program>>
{
  [Fact]
  public async Task GetCategories_ShouldReturnUnauthorized_WhenAuthorizationHeaderOmitted()
  {
    // Arrange
    using HttpClient httpClient = factory.CreateClient();
    
    // Act
    HttpResponseMessage response = await httpClient.GetAsync("api/v1/categories");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
  }
}