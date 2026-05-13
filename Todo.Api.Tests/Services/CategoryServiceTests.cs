using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Exceptions;
using Todo.Api.Models.DTOs;
using Todo.Api.Models.Entities;
using Todo.Api.Services;

namespace Todo.Api.Tests.Services;

public class CategoryServiceTests
{
  private static AppDbContext CreateContext()
  {
    DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
      .Options;

    return new AppDbContext(options);
  }

  [Fact]
  public async Task GetAll_ShouldReturnOnlyUserCategories()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    
    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();

    context.Categories.AddRange(
      new Category { Id = Guid.CreateVersion7(), Title = "Work", UserId = userId },
      new Category { Id = Guid.CreateVersion7(), Title = "Health", UserId = anotherUserId }
    );
    
    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    IReadOnlyList<CategoryResponse> result = await service.GetAll(userId);

    // Assert
    result.Should().HaveCount(1);
    result[0].Title.Should().Be("Work");
  }

  [Fact]
  public async Task GetById_ShouldReturnCategory_WhenExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid categoryId = Guid.CreateVersion7();

    context.Categories.Add(
      new Category { Id = categoryId, Title = "Sport", UserId = userId }
    );

    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    CategoryResponse result = await service.GetById(categoryId, userId);

    // Assert
    result.Title.Should().Be("Sport");
  }

  [Fact]
  public async Task GetById_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    CategoryService service = new (context);

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentCategoryId = Guid.CreateVersion7();

    // Act
    Func<Task> action = async () => { await service.GetById(nonExistentCategoryId, userId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{nonExistentCategoryId}' was not found.");
  }

  [Fact]
  public async Task GetById_ShouldThrowNotFoundException_WhenCategoryDoesNotBelognToUser()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();
    Guid categoryId = Guid.CreateVersion7();

    context.Categories.Add(
      new Category { Id = categoryId, Title = "Party", UserId = userId }
    );

    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    Func<Task> action = async () => { await service.GetById(categoryId, anotherUserId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{categoryId}' was not found.");
  }

  [Fact]
  public async Task Create_ShouldAddCategoryToDatabase_WhenDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    CategoryService service = new (context);

    Guid userId = Guid.CreateVersion7();
    CategoryRequest request = new ("  Food  ");

    // Act
    CategoryResponse result = await service.Create(userId, request);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be("Food");
    result.Id.Should().NotBeEmpty();

    Category? categoryInDb = await context.Categories.FirstOrDefaultAsync(c => c.Id == result.Id);

    categoryInDb.Should().NotBeNull();
    categoryInDb.Title.Should().Be("Food");
    categoryInDb.UserId.Should().Be(userId);
  }

  [Fact]
  public async Task Create_ShouldAddCategoryWithExistingTitleToDatabase_WhenUserIdsAreDifferent()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    
    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();
    string sharedTitle = "Hobby";

    CategoryRequest request = new (sharedTitle);
    
    context.Categories.Add(
      new Category { Id = Guid.CreateVersion7(), Title = sharedTitle, UserId = userId }
    );

    await context.SaveChangesAsync();
    
    CategoryService service = new (context);

    // Act
    CategoryResponse result = await service.Create(anotherUserId, request);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be(sharedTitle);

    List<Category> categoriesInDb = await context.Categories
      .Where(c => c.Title == sharedTitle)
      .ToListAsync();
    
    categoriesInDb.Should().HaveCount(2);
    categoriesInDb.Should().ContainSingle(c => c.UserId == anotherUserId);
  }

  [Fact]
  public async Task Create_ShouldThrowConflictException_WhenTitleExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();

    context.Categories.Add(
      new Category { Id = Guid.CreateVersion7(), Title = "Vacation", UserId = userId }
    );

    await context.SaveChangesAsync();

    CategoryService service = new (context);

    CategoryRequest request = new ("Vacation");

    // Act
    Func<Task> action = async () => { await service.Create(userId, request); };

    // Assert
    await action.Should().ThrowAsync<ConflictException>()
      .WithMessage($"Category '{request.Title}' already exists.");
  }

  [Fact]
  public async Task Update_ShouldUpdateCategoryTitle_WhenDataIsValid()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid categoryId = Guid.CreateVersion7();
    CategoryRequest request = new ("  New title  ");
    
    context.Categories.Add(
      new Category { Id = categoryId, Title = "Old title", UserId = userId }
    );

    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    await service.Update(categoryId, userId, request);

    // Assert
    Category? updatedCategory = await context.Categories
      .FirstOrDefaultAsync(c => c.Id == categoryId);

    updatedCategory.Should().NotBeNull();
    updatedCategory.Title.Should().Be("New title");
  }

  [Fact]
  public async Task Update_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    CategoryService service = new (context);

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentCategoryId = Guid.CreateVersion7();
    CategoryRequest request = new ("Concert");

    // Act
    Func<Task> action = async () => { await service.Update(nonExistentCategoryId, userId, request); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{nonExistentCategoryId}' was not found.");
  }

  [Fact]
  public async Task Update_ShouldThrowNotFoundException_WhenCategoryDoesNotBelogToUser()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid categoryId = Guid.CreateVersion7();
    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();
    
    CategoryRequest request = new ("New name");

    context.Categories.Add(
      new Category { Id = categoryId, Title = "My category", UserId = userId }
    );

    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    Func<Task> action = async () => { await service.Update(categoryId, anotherUserId, request); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{categoryId}' was not found.");
  }

  [Fact]
  public async Task Update_ShouldThrowConflictException_WhenTitleExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid categoryId = Guid.CreateVersion7();

    string sharedTitle = "Education";
    CategoryRequest request = new (sharedTitle);

    context.Categories.AddRange(
      new Category { Id = Guid.CreateVersion7(), Title = sharedTitle, UserId = userId },
      new Category { Id = categoryId, Title = "Another category", UserId = userId }
    );

    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    Func<Task> action = async () => { await service.Update(categoryId, userId, request); };

    // Assert
    await action.Should().ThrowAsync<ConflictException>()
      .WithMessage($"Category '{request.Title}' already exists.");
  }

  [Fact]
  public async Task Delete_ShouldRemoveCategoryFromDatabase_WhenExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid categoryId = Guid.CreateVersion7();

    context.Categories.Add(
      new Category { Id = categoryId, Title = "Languages", UserId = userId }
    );
    
    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    await service.Delete(categoryId, userId);

    // Assert
    Category? deletedCategory = await context.Categories.FindAsync(categoryId);
    deletedCategory.Should().BeNull();
  }

  [Fact]
  public async Task Delete_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    CategoryService service = new (context);

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentCategoryId = Guid.CreateVersion7();

    // Act
    Func<Task> action = async () => { await service.Delete(nonExistentCategoryId, userId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{nonExistentCategoryId}' was not found.");
  }

  [Fact]
  public async Task Delete_ShouldThrowNotFoundException_WhenCategoryDoesNotBelongToUser()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();
    Guid categoryId = Guid.CreateVersion7();

    context.Categories.Add(
      new Category { Id = categoryId, Title = "My Day", UserId = userId }
    );

    await context.SaveChangesAsync();

    CategoryService service = new (context);

    // Act
    Func<Task> action = async () => { await service.Delete(categoryId, anotherUserId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{categoryId}' was not found.");
  }
}