using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ToDoApi.Models.Entities;

namespace ToDoApi.Data.Configurations;

public class TaskEntityTypeConfiguration : IEntityTypeConfiguration<TaskItem>
{
  public void Configure(EntityTypeBuilder<TaskItem> builder)
  {
    builder
      .HasKey(t => t.Id);

    builder
      .Property(t => t.Title)
      .IsRequired()
      .HasMaxLength(255);

    builder
      .Property(t => t.IsCompleted)
      .HasDefaultValue(false);

    builder
      .Property(t => t.CreatedAt)
      .HasDefaultValueSql("NOW()");

    builder
      .HasOne(t => t.User)
      .WithMany(u => u.Tasks)
      .HasForeignKey(t => t.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder
      .HasOne(t => t.Category)
      .WithMany(c => c.Tasks)
      .HasForeignKey(t => t.CategoryId)
      .OnDelete(DeleteBehavior.SetNull);
  }
}