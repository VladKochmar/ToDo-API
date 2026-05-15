using Microsoft.EntityFrameworkCore;
using Todo.Api.Data.Configurations;
using Todo.Api.Models.Entities;

namespace Todo.Api.Data;

public sealed class GlobalDbContext(DbContextOptions<GlobalDbContext> options) : DbContext(options)
{
  public DbSet<Client> Clients { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    new ClientEntityTypeConfiguration().Configure(modelBuilder.Entity<Client>());
  }
}