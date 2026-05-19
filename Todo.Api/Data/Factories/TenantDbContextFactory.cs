using Microsoft.EntityFrameworkCore;

namespace Todo.Api.Data.Factories;

public class TenantDbContextFactory : ITenantDbContextFactory
{
  public AppDbContext Create(string connectionString)
  {
    DbContextOptions<AppDbContext> options = 
      new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention()
        .Options;

    return new AppDbContext(options);
  }
}