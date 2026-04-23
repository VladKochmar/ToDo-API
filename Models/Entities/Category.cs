namespace ToDoApi.Models.Entities;

public class Category
{
  public required Guid Id { get; init; }
  public required string Title { get; set; }
  public required Guid UserId { get; init; }
  public User User { get; set; } = null!;
  public List<TaskItem> Tasks { get; set; } = [];
}