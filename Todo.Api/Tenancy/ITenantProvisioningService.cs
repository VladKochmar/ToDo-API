using Todo.Api.Models.Entities;

namespace Todo.Api.Tenancy;

public interface ITenantProvisioningService
{
  /// <summary>
  /// Creates all required PostgreSQL resources for a new tenant and
  /// initializes its schema using EF Core migrations.
  /// </summary>
  /// <param name="client">
  /// Tenant metadata containing database name, username, and password.
  /// </param>
  /// <returns>Task</returns>
  public Task ProvisionTenant(Client client);
}