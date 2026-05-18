using Npgsql;

namespace Todo.Api.Data.Configurations;

public class AdminDbOptions
{
  public required string Host { get; set; }

  public required int Port { get; set; }

  public required string Database { get; set; }

  public required string Username { get; set; }

  public required string Password { get; set; }

  public string ConnectionString =>
    new NpgsqlConnectionStringBuilder
    {
      Host = Host,
      Port = Port,
      Database = Database,
      Username = Username,
      Password = Password
    }.ConnectionString;
}