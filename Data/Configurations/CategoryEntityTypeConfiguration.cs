using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoApi.Models.Entities;

namespace ToDoApi.Data.Configurations;

public class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder
      .HasKey(c => c.Id);

    builder
      .Property(c => c.Title)
      .IsRequired()
      .HasMaxLength(100);
    
    builder
      .HasOne(c => c.User)
      .WithMany(u => u.Categories)
      .HasForeignKey(c => c.UserId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}