using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class CategoryService(AppDbContext context, ICurrentUserService userService) : ICategoryService
{
  public async Task<CategoryResponse?> GetById(Guid id)
  {
    Guid userId = userService.GetUserId();

    Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    if (category is null) return null;

    return new CategoryResponse(category.Id, category.Title);
  }

  public async Task<IReadOnlyList<CategoryResponse>> GetAll()
  {
    Guid userId = userService.GetUserId();

    List<CategoryResponse> categories = await context.Categories
      .Where(c => c.UserId == userId)
      .Select(c => new CategoryResponse(c.Id, c.Title))
      .ToListAsync();

    return categories;
  }

  public async Task<CategoryResponse> Create(CategoryRequest request)
  {
    Guid userId = userService.GetUserId();

    Category newCategory = new ()
    {
      Id = Guid.CreateVersion7(),
      Title = request.Title,
      UserId = userId
    };

    context.Categories.Add(newCategory);
    await context.SaveChangesAsync();

    return new CategoryResponse(newCategory.Id, newCategory.Title);
  }

  public async Task<bool> Update(Guid id, CategoryRequest request)
  {
    Guid userId = userService.GetUserId();

    Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    if (category is null) return false;

    category.Title = request.Title;
    await context.SaveChangesAsync();

    return true;
  }

  public async Task<bool> Delete(Guid id)
  {
    Guid userId = userService.GetUserId();
    
    Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    if (category is null) return false;

    context.Categories.Remove(category);
    await context.SaveChangesAsync();

    return true;
  }
}