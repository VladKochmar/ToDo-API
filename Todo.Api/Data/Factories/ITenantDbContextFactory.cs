namespace Todo.Api.Data.Factories;

public interface ITenantDbContextFactory
{
  /// <summary>
  /// Creates a new <see cref="AppDbContext"/> configured for a specific tenant database.
  /// </summary>
  /// <param name="connectionString">
  /// PostgreSQL connection string to a tenant database. 
  /// </param>
  /// <returns>
  /// A new instance of <see cref="AppDbContext"/> configured for the specified tenatn.
  /// </returns>
  public AppDbContext Create(string connectionString);
}