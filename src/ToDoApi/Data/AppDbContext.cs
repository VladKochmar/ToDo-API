using Microsoft.EntityFrameworkCore;
using ToDoApi.Data.Configurations;
using ToDoApi.Models.Entities;

namespace ToDoApi.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<User> Users { get; set; }
  public DbSet<Category> Categories { get; set; }
  public DbSet<TaskItem> Tasks { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    new UserEntityTypeConfiguration().Configure(modelBuilder.Entity<User>());
    new CategoryEntityTypeConfiguration().Configure(modelBuilder.Entity<Category>());
    new TaskEntityTypeConfiguration().Configure(modelBuilder.Entity<TaskItem>());
  }
}