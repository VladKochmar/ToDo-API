using Microsoft.EntityFrameworkCore;
using ToDoApi.Data;
using ToDoApi.Exceptions;
using ToDoApi.Models.DTOs;
using ToDoApi.Models.Entities;

namespace ToDoApi.Services;

public class CategoryService(AppDbContext context, IUserContext userContext) : ICategoryService
{
  public async Task<CategoryResponse> GetById(Guid id)
  {
    string authId = userContext.AuthId();
    Guid userId = await context.Users
      .Where(u => u.AuthId == authId)
      .Select(u => u.Id)
      .FirstOrDefaultAsync();

    Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    
    if (category is null)
      throw new NotFoundException("Category with the given ID was not found.");

    return new CategoryResponse(category.Id, category.Title);
  }

  public async Task<IReadOnlyList<CategoryResponse>> GetAll()
  {
    string authId = userContext.AuthId();
    Guid userId = await context.Users
      .Where(u => u.AuthId == authId)
      .Select(u => u.Id)
      .FirstOrDefaultAsync();

    List<CategoryResponse> categories = await context.Categories
      .Where(c => c.UserId == userId)
      .Select(c => new CategoryResponse(c.Id, c.Title))
      .ToListAsync();

    return categories;
  }

  public async Task<CategoryResponse> Create(CategoryRequest request)
  {
    string authId = userContext.AuthId();
    Guid userId = await context.Users
      .Where(u => u.AuthId == authId)
      .Select(u => u.Id)
      .FirstOrDefaultAsync();

    bool exists = await context.Categories.AnyAsync(c => c.Title == request.Title && c.UserId == userId);
    
    if (exists)
      throw new ConflictException($"Category '{request.Title}' already exists.");

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

  public async Task Update(Guid id, CategoryRequest request)
  {
    string authId = userContext.AuthId();
    Guid userId = await context.Users
      .Where(u => u.AuthId == authId)
      .Select(u => u.Id)
      .FirstOrDefaultAsync();

    Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    
    if (category is null)
      throw new NotFoundException("Category with the given ID was not found.");

    bool exists = await context.Categories.AnyAsync(c => c.Title == request.Title && 
      c.UserId == userId && c.Id != id);

    if (exists)
      throw new ConflictException($"Category '{request.Title}' already exists.");

    category.Title = request.Title;
    await context.SaveChangesAsync();
  }

  public async Task Delete(Guid id)
  {
    string authId = userContext.AuthId();
    Guid userId = await context.Users
      .Where(u => u.AuthId == authId)
      .Select(u => u.Id)
      .FirstOrDefaultAsync();
    
    Category? category = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    
    if (category is null)
      throw new NotFoundException("Category with the given ID was not found.");

    context.Categories.Remove(category);
    await context.SaveChangesAsync();
  }
}