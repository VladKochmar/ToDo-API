using Npgsql;

using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

using Todo.Api.Data;
using Todo.Api.Data.Factories;
using Todo.Api.Data.Configurations;

using Todo.Api.Models.Entities;

namespace Todo.Api.Services;

public class TenantProvisioningService
(
  ITenantDbContextFactory factory,
  IOptions<AdminDbOptions> options
) : ITenantProvisioningService
{
  public async Task ProvisionTenant(Client client)
  {
    string adminConnectionString = options.Value.ConnectionString;

    await using NpgsqlConnection connection = new(adminConnectionString);

    await connection.OpenAsync();

    await CreateUser(connection, client);

    await CreateDatabase(connection, client);

    await SetupGrants(connection, client);

    await SetupSchemaGrants(client);

    await ApplyMigrations(client);
  }

  private static async Task CreateUser(NpgsqlConnection connection, Client client)
  {
    string createUserSql = 
    $"""
    CREATE USER "{client.DbUser}"
    WITH PASSWORD '{client.DbPassword}';
    """;

    await using NpgsqlCommand command = new(createUserSql, connection);
    await command.ExecuteNonQueryAsync();
  }

  private static async Task CreateDatabase(NpgsqlConnection connection, Client client)
  {
    string createDbSql = 
    $"""
    CREATE DATABASE "{client.DbName}"
    OWNER "{client.DbUser}";
    """;

    await using NpgsqlCommand command = new(createDbSql, connection);
    await command.ExecuteNonQueryAsync();
  }

  private static async Task SetupGrants(NpgsqlConnection connection, Client client)
  {
    string setupGrantsSql =
    $"""
    GRANT ALL PRIVILEGES
    ON DATABASE "{client.DbName}"
    TO "{client.DbUser}";
    """;

    await using NpgsqlCommand command = new(setupGrantsSql, connection);
    await command.ExecuteNonQueryAsync();
  }

  private async Task SetupSchemaGrants(Client client)
  {
    string adminUser = options.Value.Username;
    string adminPassword = options.Value.Password;
    
    string adminTenantConnectionString = $"Host=localhost;Port=5432;Database={client.DbName};Username={adminUser};Password={adminPassword}";

    await using NpgsqlConnection connection = new(adminTenantConnectionString);

    await connection.OpenAsync();

    string setupSchemaGrantsSql = 
    $"""
    GRANT ALL ON SCHEMA public TO "{client.DbUser}";
    """;

    await using NpgsqlCommand command = new(setupSchemaGrantsSql, connection);
    await command.ExecuteNonQueryAsync();
  }

  private async Task ApplyMigrations(Client client)
  {
    string tenantConnectionString = $"Host=localhost;Port=5432;Database={client.DbName};Username={client.DbUser};Password={client.DbPassword}";
    
    await using AppDbContext context = factory.Create(tenantConnectionString);

    await context.Database.MigrateAsync();
  }
}