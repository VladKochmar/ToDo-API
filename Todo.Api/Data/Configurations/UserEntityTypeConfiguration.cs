using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo.Api.Models.Entities;

namespace Todo.Api.Data.Configurations;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder
      .HasKey(u => u.Id);

    builder.Property(u => u.AuthId)
      .IsRequired()
      .HasMaxLength(255);

    builder
      .HasIndex(u => u.AuthId)
      .IsUnique();
    
    builder
      .Property(u => u.FullName)
      .IsRequired()
      .HasMaxLength(255);
    
    builder
      .Property(u => u.Email)
      .IsRequired()
      .HasMaxLength(255);
    
    builder
      .HasIndex(u => u.Email)
      .IsUnique();
  }
}