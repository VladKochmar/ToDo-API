namespace Todo.Api.Tenancy;

public class TenantInfo
{
  public Guid ClientId { get; set; }

  public required string ConnectionString { get; init; }
}