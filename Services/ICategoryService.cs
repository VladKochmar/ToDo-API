using ToDoApi.Models.DTOs;

namespace ToDoApi.Services;

public interface ICategoryService
{
  public Task<CategoryResponse> GetById(Guid id);
  public Task<IReadOnlyList<CategoryResponse>> GetAll();
  public Task<CategoryResponse> Create(CategoryRequest request);
  public Task Update(Guid id, CategoryRequest request);
  public Task Delete(Guid id);
}