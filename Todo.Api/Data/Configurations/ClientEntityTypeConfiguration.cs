using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo.Api.Models.Entities;

namespace Todo.Api.Data.Configurations;

public class ClientEntityTypeConfiguration : IEntityTypeConfiguration<Client>
{
  public void Configure(EntityTypeBuilder<Client> builder)
  {
    builder
      .HasKey(c => c.Id);

    builder
      .Property(c => c.Name)
      .IsRequired()
      .HasMaxLength(100);

    builder
      .Property(c => c.DbUser)
      .IsRequired()
      .HasMaxLength(255);

    builder
      .Property(c => c.DbName)
      .IsRequired()
      .HasMaxLength(255);

    builder
      .Property(c => c.DbPassword)
      .IsRequired()
      .HasMaxLength(255);
  }
}