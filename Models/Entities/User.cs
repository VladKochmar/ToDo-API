namespace ToDoApi.Models.Entities;

public class User
{
  public required Guid Id { get; init; }
  public required string AuthId { get; init; }
  public required string FirstName { get; set; }
  public required string LastName { get; set; }
  public required string Email { get; set; }
  public List<Category> Categories { get; set; } = [];
  public List<TaskItem> Tasks { get; set; } = [];
}