using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Exceptions;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;
using ToDoApi.Services;

namespace TodoApi.Tests.Services;

public class TaskServiceTests
{
  private static AppDbContext CreateContext()
  {
    DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
      .Options;

    return new AppDbContext(options);
  }

  [Fact]
  public async Task Create_ShouldAddTaskToDatabase_WhenCategoryIdsAreDifferent()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid categoryId = Guid.CreateVersion7();
    Guid anotherCategoryId = Guid.CreateVersion7();
    string sharedTitle = "To warm up";

    CreateTaskRequest request = new (sharedTitle, null, null, categoryId);

    context.Categories.AddRange(
      new Category { Id = categoryId, Title = "Sport", UserId = userId },
      new Category { Id = anotherCategoryId, Title = "Health", UserId = userId  }
    );

    context.Tasks.Add(
      new TaskItem { Id = Guid.CreateVersion7(), Title = sharedTitle, UserId = userId, CategoryId = anotherCategoryId }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    TaskResponse result = await service.Create(userId, request);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be(request.Title);
    result.Description.Should().Be(request.Description);
    result.IsCompleted.Should().BeFalse();

    List<TaskItem> tasksInDb = await context.Tasks
      .Where(t => t.Title == sharedTitle)
      .ToListAsync();

    tasksInDb.Should().HaveCount(2);
    tasksInDb.Should().ContainSingle(t => t.CategoryId == anotherCategoryId);
  }

  [Fact]
  public async Task Create_ShouldAddTaskToDatabase_WhenUserIdsAreDifferent()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();
    string sharedTitle = "Develop ToDo API";

    CreateTaskRequest request = new (sharedTitle, "Some description", null, null);

    TaskItem taskItem = new () { Id = Guid.CreateVersion7(), Title = sharedTitle, UserId = anotherUserId };

    context.Tasks.Add(taskItem);
    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    TaskResponse result = await service.Create(userId, request);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be(request.Title);
    result.Description.Should().Be(request.Description);
    result.IsCompleted.Should().BeFalse();

    List<TaskItem> tasksInDb = await context.Tasks
      .Where(t => t.Title == sharedTitle)
      .ToListAsync();

    tasksInDb.Should().HaveCount(2);
    tasksInDb.Should().ContainSingle(t => t.UserId == anotherUserId);
  }

  [Fact]
  public async Task Create_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    TaskService service = new (context);

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentCategoryId = Guid.CreateVersion7();
    
    CreateTaskRequest request = new ("Play football", null, null, nonExistentCategoryId);

    // Act
    Func<Task> action = async () => { await service.Create(userId, request); };
    
    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{nonExistentCategoryId}' was not found.");
  }

  [Fact]
  public async Task Create_ShouldThrowNotFoundException_WhenCategoryDoesNotBelongToUser()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();

    Category category = new () { Id = Guid.CreateVersion7(), Title = "Sport", UserId = anotherUserId };
    
    CreateTaskRequest request = new ("Play football", null, null, category.Id);

    context.Categories.Add(category);

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    Func<Task> action = async () => { await service.Create(userId, request); };
    
    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{category.Id}' was not found.");
  }

  [Fact]
  public async Task Create_ShouldThrowConflictException_WhenTaskTitleExistsInSameCategory()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    string sharedTitle = "Learn RxJs";

    Category category = new () { Id = Guid.CreateVersion7(), Title = "Work", UserId = userId };
    TaskItem taskItem = new () { Id = Guid.CreateVersion7(), Title = sharedTitle, UserId = userId, CategoryId = category.Id };

    CreateTaskRequest request = new (sharedTitle, null, null, category.Id);

    context.Categories.Add(category);
    context.Tasks.Add(taskItem);

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    Func<Task> action = async () => { await service.Create(userId, request); };

    // Assert
    await action.Should().ThrowAsync<ConflictException>()
      .WithMessage($"Task '{request.Title}' already exists.");
  }

  [Fact]
  public async Task Update_ShouldUpdateTask_WhenRequestIsValid()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();

    Category oldCategory = new () { Id = Guid.CreateVersion7(), Title = "Old category", UserId = userId };
    Category newCategory = new () { Id = Guid.CreateVersion7(), Title = "New category", UserId = userId };
    
    TaskItem taskItem = new () { 
      Id = Guid.CreateVersion7(), 
      Title = "Old title",
      Description = "Old description",
      UserId = userId, 
      CategoryId = oldCategory.Id ,
      DueDate = DateTimeOffset.UtcNow
    };

    context.Categories.AddRange(
      oldCategory,
      newCategory
    );

    context.Tasks.Add(taskItem);

    await context.SaveChangesAsync();

    TaskService service = new (context);

    UpdateTaskRequest request = new (
      "   New title   ",
      true,
      "  New description  ",
      DateTimeOffset.UtcNow.AddDays(5),
      newCategory.Id
    );

    // Act
    await service.Update(taskItem.Id, userId, request);

    // Assert
    TaskItem? updatedTask = await context.Tasks
      .FirstOrDefaultAsync(t => t.Id == taskItem.Id);

    updatedTask.Should().NotBeNull();

    updatedTask.Title.Should().Be("New title");
    updatedTask.Description.Should().Be("New description");
    updatedTask.IsCompleted.Should().BeTrue();
    updatedTask.CategoryId.Should().Be(request.CategoryId);
    updatedTask.DueDate.Should().Be(request.DueDate);
  }

  [Fact]
  public async Task Update_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    TaskService service = new (context);

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentTaskId = Guid.CreateVersion7();

    UpdateTaskRequest request = new (
      "Non Existent Task",
      false,
      null,
      null,
      null
    );

    // Act
    Func<Task> action = async () => { 
      await service.Update(nonExistentTaskId, userId, request); 
    };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Task with ID '{nonExistentTaskId}' was not found.");;
  }

  [Fact]
  public async Task Update_ShouldThrowNotFoundException_WhenNewCategoryDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentCategoryId = Guid.CreateVersion7();

    Category category = new () { Id = Guid.CreateVersion7(), Title = "Work", UserId = userId };
    
    TaskItem taskItem = new () 
    { 
      Id = Guid.CreateVersion7(), 
      Title = "Title", 
      UserId = userId, 
      CategoryId = category.Id 
    };

    context.Categories.Add(category);
    context.Tasks.Add(taskItem);

    await context.SaveChangesAsync();

    TaskService service = new (context);

    UpdateTaskRequest request = new (
      "Updated",
      false,
      null,
      null,
      nonExistentCategoryId
    );

    // Act
    Func<Task> action = async () =>
    {
      await service.Update(taskItem.Id, userId, request);
    };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{nonExistentCategoryId}' was not found.");
  }

  [Fact]
  public async Task Update_ShouldThrowConflictException_WhenTaskAlreadyCompleted()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();

    TaskItem taskItem = new ()
    {
      Id = Guid.CreateVersion7(),
      Title = "Completed task",
      IsCompleted = true,
      UserId = userId
    };

    context.Tasks.Add(taskItem);

    await context.SaveChangesAsync();

    TaskService service = new (context);

    UpdateTaskRequest request = new (
      "Updated",
      false,
      null,
      null,
      null
    );

    // Act
    Func<Task> action = async () => { await service.Update(taskItem.Id, userId, request); };

    // Assert
    await action.Should().ThrowAsync<ConflictException>()
      .WithMessage("Task already completed.");
  }

  [Fact]
  public async Task Update_ShouldThrowConflictException_WhenDuplicateTaskExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();

    Category category = new () { Id = Guid.CreateVersion7(), Title = "Work", UserId = userId };

    TaskItem existingTask = new () 
    { 
      Id = Guid.CreateVersion7(), 
      Title = "Existing", 
      UserId = userId, 
      CategoryId = category.Id 
    };

    TaskItem taskToUpdate = new () 
    { 
      Id = Guid.CreateVersion7(), 
      Title = "Old title", 
      UserId = userId, 
      CategoryId = category.Id 
    };

    context.Categories.Add(category);
    context.Tasks.AddRange(existingTask, taskToUpdate);

    await context.SaveChangesAsync();

    TaskService service = new (context);

    UpdateTaskRequest request = new (
      "Existing",
      false,
      null,
      null,
      category.Id
    );

    // Act
    Func<Task> action = async () => 
    { 
      await service.Update(taskToUpdate.Id, userId, request); 
    };

    // Assert
    await action.Should()
      .ThrowAsync<ConflictException>()
      .WithMessage($"Task '{request.Title}' already exists.");
  }

  [Fact]
  public async Task GetAll_ShouldReturnAllUserTasks()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();

    context.Tasks.AddRange(
      new TaskItem { Id = Guid.CreateVersion7(), Title = "Learn C#", UserId = userId },
      new TaskItem { Id = Guid.CreateVersion7(), Title = "Learn Angular", UserId = anotherUserId }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    IReadOnlyList<TaskResponse> result = await service.GetAll(userId);

    // Assert
    result.Should().HaveCount(1);
    result[0].Title.Should().Be("Learn C#");
  }

  [Fact]
  public async Task GetAllByCategory_ShouldReturnAllUserTasks_WhenBelongToUserAndCategory()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();

    Category category = new () { Id = Guid.CreateVersion7(), Title = "Work", UserId = userId };

    context.Categories.Add(category);
    context.Tasks.AddRange(
      new TaskItem { Id = Guid.CreateVersion7(), Title = "Go for a walk", UserId = userId },
      new TaskItem { Id = Guid.CreateVersion7(), Title = "Test ToDo App", UserId = userId, CategoryId = category.Id }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    IReadOnlyList<TaskResponse> result = await service.GetAllByCategory(category.Id, userId);

    // Assert
    result.Should().HaveCount(1);
    result[0].Title.Should().Be("Test ToDo App");
    result[0].CategoryName.Should().Be("Work");
  }

  [Fact]
  public async Task GetAllByCategory_ShouldThrowNotFoundException_WhenCategoryDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentCategoryId = Guid.CreateVersion7();

    context.Tasks.AddRange(
      new TaskItem { Id = Guid.CreateVersion7(), Title = "Learn English", UserId = userId, CategoryId = Guid.CreateVersion7() },
      new TaskItem { Id = Guid.CreateVersion7(), Title = "Learn Polish", UserId = userId }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    Func<Task> action = async () => { await service.GetAllByCategory(nonExistentCategoryId, userId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{nonExistentCategoryId}' was not found.");
  }

  [Fact]
  public async Task GetAllByCategory_ShouldThrowNotFoundException_WhenCategoryDoesNotBelongToUser()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();

    Category category = new () { Id = Guid.CreateVersion7(), Title = "Some category", UserId = anotherUserId };

    context.Categories.Add(category);
    context.Tasks.AddRange(
      new TaskItem { Id = Guid.CreateVersion7(), Title = "My task", UserId = userId, CategoryId = category.Id },
      new TaskItem { Id = Guid.CreateVersion7(), Title = "Foreign task", UserId = anotherUserId, CategoryId = category.Id }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    Func<Task> action = async () => { await service.GetAllByCategory(category.Id, userId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Category with ID '{category.Id}' was not found.");
  }

  [Fact]
  public async Task GetById_ShouldReturnTask_WhenExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    
    Guid userId = Guid.CreateVersion7();
    Guid taskId = Guid.CreateVersion7();

    Category category = new () { Id = Guid.CreateVersion7(), Title = "Sport", UserId = userId };

    context.Categories.Add(category);
    context.Tasks.Add(
      new TaskItem { Id = taskId, Title = "Play football", UserId = userId, CategoryId = category.Id }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    TaskResponse result = await service.GetById(taskId, userId);

    // Assert
    result.Should().NotBeNull();
    result.Title.Should().Be("Play football");
    result.CategoryName.Should().Be("Sport");
  }

  [Fact]
  public async Task GetById_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    TaskService service = new (context);

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentTaskId = Guid.CreateVersion7();

    // Act
    Func<Task> action = async () => { await service.GetById(nonExistentTaskId, userId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Task with ID '{nonExistentTaskId}' was not found.");
  }

  [Fact]
  public async Task GetById_ShouldThrowNotFoundException_WhenTaskDoesNotBelongToUser()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();
    Guid taskId = Guid.CreateVersion7();

    context.Tasks.Add(
      new TaskItem { Id = taskId, Title = "Watch basketball", UserId = userId }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);
    // Act
    Func<Task> action = async () => { await service.GetById(taskId, anotherUserId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Task with ID '{taskId}' was not found.");
  }

  [Fact]
  public async Task Delete_ShouldRemoveTaskFromDatabase_WhenExists()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid taskId = Guid.CreateVersion7();

    context.Tasks.Add(
      new TaskItem { Id = taskId, Title = "Buy milk", UserId = userId }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);

    // Act
    await service.Delete(taskId, userId);

    // Assert
    TaskItem? deletedTask = await context.Tasks.FindAsync(taskId);
    deletedTask.Should().BeNull();
  }

  [Fact]
  public async Task Delete_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
  {
    // Arrange
    using AppDbContext context = CreateContext();
    TaskService service = new (context);

    Guid userId = Guid.CreateVersion7();
    Guid nonExistentTaskId = Guid.CreateVersion7();

    // Act
    Func<Task> action = async () => { await service.Delete(nonExistentTaskId, userId); };

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Task with ID '{nonExistentTaskId}' was not found.");
  }

  [Fact]
  public async Task Delete_ShouldThrowNotFoundException_WhenTaskDoesNotBelongToUser()
  {
    // Arrange
    using AppDbContext context = CreateContext();

    Guid userId = Guid.CreateVersion7();
    Guid anotherUserId = Guid.CreateVersion7();
    Guid taskId = Guid.CreateVersion7();

    context.Tasks.Add(
      new TaskItem { Id = taskId, Title = "Watch movie", UserId = userId }
    );

    await context.SaveChangesAsync();

    TaskService service = new (context);
    // Act
    Func<Task> action = async () => await service.Delete(taskId, anotherUserId);

    // Assert
    await action.Should().ThrowAsync<NotFoundException>()
      .WithMessage($"Task with ID '{taskId}' was not found.");
  }
}