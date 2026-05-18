using Microsoft.EntityFrameworkCore;

namespace Todo.Api.Data.Factories;

public class TenantDbContextFactory : ITenantDbContextFactory
{
  public AppDbContext Create(string connectionString)
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
      .UseNpgsql(connectionString)
      .UseSnakeCaseNamingConvention()
      .Options;

    return new AppDbContext(options);
  }
}