namespace ToDoApi.Exceptions;

public class NotFoundException : Exception
{
  public Guid EntityId { get; }
  public string? EntityName { get; }

  public NotFoundException() : base("The required entity was not found.") {}

  public NotFoundException(Guid entityId, string? entityName = null) 
    : base(BuildMessage(entityId, entityName))
  {
    EntityId = entityId;
    EntityName = entityName;
  }

  public NotFoundException(Guid entityId, Exception innerException)
    : base(BuildMessage(entityId, null), innerException)
  {
    EntityId = entityId;
  }

  public NotFoundException(Guid entityId, string? entityName, Exception innerException)
    : base(BuildMessage(entityId, entityName), innerException)
  {
    EntityId = entityId;
    EntityName = entityName;
  }

  public NotFoundException(string message) : base(message) {}

  public NotFoundException(string message, Exception innerException) : base(message, innerException) {}

  private static string BuildMessage(Guid id, string? name)
  {
    return name is null
      ? $"Entity with ID '{id}' was not found."
      : $"{name} with ID '{id}' was not found.";
  }
}