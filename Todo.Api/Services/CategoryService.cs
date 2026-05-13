using Microsoft.EntityFrameworkCore;
using Todo.Api.Data;
using Todo.Api.Exceptions;
using Todo.Api.Models.DTOs;
using Todo.Api.Models.Entities;

namespace Todo.Api.Services;

public class CategoryService(AppDbContext context) : ICategoryService
{
  public async Task<CategoryResponse> GetById(Guid id, Guid userId)
  {
    Category category = await GetCategoryOrThrow(id, userId);
    return new CategoryResponse(category.Id, category.Title);
  }

  public async Task<IReadOnlyList<CategoryResponse>> GetAll(Guid userId)
  {
    List<CategoryResponse> categories = await context.Categories
      .Where(c => c.UserId == userId)
      .Select(c => new CategoryResponse(c.Id, c.Title))
      .ToListAsync();

    return categories;
  }

  public async Task<CategoryResponse> Create(Guid userId, CategoryRequest request)
  {
    bool exists = await context.Categories.AnyAsync(c => c.Title == request.Title && c.UserId == userId);
    
    if (exists)
      throw new ConflictException($"Category '{request.Title}' already exists.");

    Category newCategory = new ()
    {
      Id = Guid.CreateVersion7(),
      Title = request.Title.Trim(),
      UserId = userId
    };

    context.Categories.Add(newCategory);
    await context.SaveChangesAsync();

    return new CategoryResponse(newCategory.Id, newCategory.Title);
  }

  public async Task Update(Guid id, Guid userId, CategoryRequest request)
  {
    Category category = await GetCategoryOrThrow(id, userId);

    bool exists = await context.Categories.AnyAsync(c => c.Title == request.Title && 
      c.UserId == userId && c.Id != id);

    if (exists)
      throw new ConflictException($"Category '{request.Title}' already exists.");

    category.Title = request.Title.Trim();
    await context.SaveChangesAsync();
  }

  public async Task Delete(Guid id, Guid userId)
  {
    Category category = await GetCategoryOrThrow(id, userId);

    context.Categories.Remove(category);
    await context.SaveChangesAsync();
  }

  private async Task<Category> GetCategoryOrThrow(Guid categoryId, Guid userId)
  {
    Category? category = await context.Categories
      .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

    if (category is null)
      throw new NotFoundException(categoryId, "Category");

    return category;
  }
}