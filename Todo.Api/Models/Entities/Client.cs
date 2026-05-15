namespace Todo.Api.Models.Entities;

public class Client
{
  public required Guid Id { get; init; }

  public required string Name { get; set; }

  public required string DbName { get; init; }

  public required string DbUser { get; init; }

  public required string DbPassword { get; init; }
}