using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Moq;
using ToDoApi.Data;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;
using ToDoApi.Services;

namespace TodoApi.Tests.Services;

public class UserServiceTest
{
  private static AppDbContext CreateContext()
  {
    DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
      .Options;

    return new AppDbContext(options);
  }

  private static string CreateJwtToken(string sub, string email, string name)
  {
    JwtSecurityToken token = new(
      claims: 
      [
        new Claim(JwtRegisteredClaimNames.Sub, sub),
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim(JwtRegisteredClaimNames.Name, name)
      ]
    );

    return new JwtSecurityTokenHandler()
      .WriteToken(token);
  }

  [Fact]
  public async Task SyncUser_ShouldCreateUser_WhenUserDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    
    Mock<IValidator<CreateUserRequest>> validatorMock = new();
    validatorMock
    .Setup(v => v.ValidateAsync(
        It.IsAny<CreateUserRequest>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ValidationResult());
    
    UserService service = new(context, validatorMock.Object);

    string authId = "auth-id-123";
    string userEmail = "user@gmail.com";
    string userName = "User";

    string token = CreateJwtToken(authId, userEmail, userName);

    // Act
    await service.SyncUser(token);

    // Assert
    User? user = await context.Users.FirstOrDefaultAsync(u => u.AuthId == authId);

    user.Should().NotBeNull();
    user.AuthId.Should().Be(authId);
    user.Email.Should().Be(userEmail);
    user.FullName.Should().Be(userName);
  }

  [Fact]
  public async Task SyncUser_ShouldUpdateUser_WhenUserExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    string authId = "auth-id-123";

    context.Users.Add(
      new User 
      { 
        Id = Guid.CreateVersion7(), 
        AuthId = authId, 
        Email = "old.user@gamil.com", 
        FullName = "Old Name"
      }
    );

    await context.SaveChangesAsync();

    Mock<IValidator<CreateUserRequest>> validatorMock = new();
    validatorMock
    .Setup(v => v.ValidateAsync(
        It.IsAny<CreateUserRequest>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ValidationResult());

    UserService service = new(context, validatorMock.Object);

    string newUserEmail = "new.user@gmail.com";
    string newUserName = "New Name";

    string token = CreateJwtToken(authId, newUserEmail, newUserName);

    // Act
    await service.SyncUser(token);

    // Assert
    User? user = await context.Users.FirstOrDefaultAsync(u => u.AuthId == authId);

    user!.Email.Should().Be(newUserEmail);
    user!.FullName.Should().Be(newUserName);
  }

  [Fact]
  public async Task SyncUser_ShouldThrowArgumentException_WhenTokenIsInvalid()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    Mock<IValidator<CreateUserRequest>> validatorMock = new();
    UserService service = new(context, validatorMock.Object);

    string invalidToken = "invalid-token";

    // Act
    Func<Task> action = async () => 
    { 
      await service.SyncUser(invalidToken); 
    };

    // Assert
    await action.Should().ThrowAsync<ArgumentException>()
      .WithMessage("Invalid token format.");
  }
}