namespace ToDoApi.Exceptions;

public class NotFoundException : Exception
{
  public Guid EntityId { get; }
  public NotFoundException(string message) : base(message) {}
  public NotFoundException(string message, Exception innerException) : base(message, innerException) {}
}