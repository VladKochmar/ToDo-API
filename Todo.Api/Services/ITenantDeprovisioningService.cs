using Todo.Api.Models.Entities;

namespace Todo.Api.Services;

public interface ITenantDeprovisioningService
{
  /// <summary>
  /// Performs a full tenant deprovisioning by terminating all active connections,
  /// dropping the dedicated database, and removing the associated database user.
  /// </summary>
  /// <param name="client">
  /// Tenant metadata containing database name, username, and password.
  /// </param>
  /// <returns>Task</returns>
  public Task DropTenant(Client client);
}