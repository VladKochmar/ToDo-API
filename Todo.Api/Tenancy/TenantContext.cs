namespace Todo.Api.Tenancy;

public class TenantContext
{
  public TenantInfo? CurrentTenant { get; set; }
}