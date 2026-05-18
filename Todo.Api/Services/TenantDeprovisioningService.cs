using Microsoft.Extensions.Options;
using Npgsql;
using Todo.Api.Data.Configurations;
using Todo.Api.Models.Entities;

namespace Todo.Api.Services;

public class TenantDeprovisioningService
(
  IOptions<AdminDbOptions> options
) : ITenantDeprovisioningService
{
  public async Task DropTenant(Client client)
  {
    string adminConnectionString = options.Value.ConnectionString;

    await using NpgsqlConnection connection = new(adminConnectionString);

    await connection.OpenAsync();

    await TerminateActiveConnections(connection, client);

    await DropDatabase(connection, client);

    await DropUser(connection, client);
  }

  private static async Task TerminateActiveConnections(NpgsqlConnection connection, Client client)
  {
    string dbParameterName = "@dbName";

    string terminateConnectionsSql = 
    $"""
    SELECT pg_terminate_backend(pid)
    FROM pg_stat_activity
    WHERE datname = {dbParameterName}
    AND pid <> pg_backend_pid();
    """;

    using NpgsqlCommand command = new(terminateConnectionsSql, connection);

    NpgsqlParameter dbNameParam = new(dbParameterName, client.DbName);
    command.Parameters.Add(dbNameParam);

    await command.ExecuteNonQueryAsync();
  }

  private static async Task DropDatabase(NpgsqlConnection connection, Client client)
  {
    string dropDbSql = 
    $"""
    DROP DATABASE "{client.DbName}";
    """;

    using NpgsqlCommand command = new(dropDbSql, connection);
    await command.ExecuteNonQueryAsync();
  }

  private static async Task DropUser(NpgsqlConnection connection, Client client)
  {
    string dropUserSql =
    $"""
    DROP USER "{client.DbUser}";
    """;

    using NpgsqlCommand command = new(dropUserSql, connection);
    await command.ExecuteNonQueryAsync();
  }
}