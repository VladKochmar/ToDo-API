using Todo.Api.Models.DTOs;

namespace Todo.Api.Services;

public interface IDbCredentialsGenerator
{
  /// <summary>
  /// Generates a new set of credentials for a tenant database.
  /// </summary>
  /// <returns>
  /// A <see cref="DbCredentials"/> object.
  /// </returns>
  public DbCredentials Generate();
}