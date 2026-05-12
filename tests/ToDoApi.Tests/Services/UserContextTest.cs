using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using ToDoApi.Data;
using ToDoApi.Models.Entities;
using ToDoApi.Services;

namespace TodoApi.Tests.Services;

public class UserContextTest
{
  private static AppDbContext CreateContext()
  {
    DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
      .Options;

    return new AppDbContext(options);
  }

  [Fact]
  public async Task UserId_ShouldReturnUserId_WhenUserExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    string authId = "auth-id-123";

    context.Users.Add(
      new User { Id = userId, AuthId = authId, Email = "user@gmail.com", FullName = "User" }
    );

    await context.SaveChangesAsync();

    List<Claim> claims = [
      new Claim(ClaimTypes.NameIdentifier, authId)
    ];

    ClaimsIdentity identity = new (claims);
    ClaimsPrincipal principal = new (identity);

    DefaultHttpContext httpContext = new()
    {
      User = principal
    };

    Mock<IHttpContextAccessor> accessorMock = new();

    accessorMock
      .Setup(a => a.HttpContext)
      .Returns(httpContext);

    UserContext service = new (accessorMock.Object, context);

    // Act
    Guid result = await service.UserId();

    // Assert
    result.Should().Be(userId);
  }

  [Fact]
  public async Task UserId_ShouldThrowUnauthorizedException_WhenClaimMissing()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    DefaultHttpContext httpContext = new();
    
    Mock<IHttpContextAccessor> accessorMock = new();

    accessorMock
      .Setup(a => a.HttpContext)
      .Returns(httpContext);

    UserContext service = new (accessorMock.Object, context);

    // Act
    Func<Task> action = async () => await service.UserId();

    // Assert
    await action.Should()
      .ThrowAsync<UnauthorizedAccessException>()
      .WithMessage("Missing sub claim.");
  }

  [Fact]
  public async Task UserId_ShouldThrowInvalidOperationException_WhenUserDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    string authId = "auth-id-123";

    List<Claim> claims = [
      new Claim(ClaimTypes.NameIdentifier, authId)
    ];

    ClaimsIdentity identity = new (claims);
    ClaimsPrincipal principal = new (identity);

    DefaultHttpContext httpContext = new()
    {
      User = principal
    };

    Mock<IHttpContextAccessor> accessorMock = new();

    accessorMock
      .Setup(a => a.HttpContext)
      .Returns(httpContext);

    UserContext service = new (accessorMock.Object, context);

    // Act
    Func<Task> action = async () =>
    {
      await service.UserId();
    };

    // Assert
    await action.Should()
      .ThrowAsync<InvalidOperationException>()
      .WithMessage("Authenticated user does not exist in the database.");
  }
}