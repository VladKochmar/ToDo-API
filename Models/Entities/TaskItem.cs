namespace ToDoApi.Models.Entities;

public class TaskItem
{
  public required Guid Id { get; init; }
  public required string Title { get; set; }
  public bool IsCompleted { get; set; } = false;
  
  public string? Description { get; set; }
  public DateTimeOffset? DueDate { get; set; }
  public required DateTimeOffset CreatedAt { get; init; }
  
  public required Guid UserId { get; set; }
  public User User { get; set; } = null!;
  
  public Guid? CategoryId { get; set; }
  public Category? Category { get; set; }
}