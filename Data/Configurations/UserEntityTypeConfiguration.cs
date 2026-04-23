using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoApi.Models.Entities;

namespace ToDoApi.Data.Configurations;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder
      .HasKey(u => u.Id);
    
    builder
      .Property(u => u.FirstName)
      .IsRequired()
      .HasMaxLength(100);
    
    builder
      .Property(u => u.LastName)
      .IsRequired()
      .HasMaxLength(100);
    
    builder
      .Property(u => u.Email)
      .IsRequired()
      .HasMaxLength(255);
    
    builder
      .HasIndex(u => u.Email)
      .IsUnique();
    
    builder
      .Property(u => u.PasswordHash)
      .IsRequired()
      .HasMaxLength(255);
  }
}