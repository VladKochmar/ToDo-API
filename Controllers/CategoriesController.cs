using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models.DTOs;
using ToDoApi.Services;

namespace ToDoApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryService service, IValidator<CategoryRequest> validator) : ControllerBase
{
  [HttpGet]
  public async Task<IActionResult> GetCategories() => Ok(await service.GetAll());

  [HttpGet("{id:guid}")]
  public async Task<IActionResult> GetCategoryById(Guid id) =>
    Ok(await service.GetById(id));

  [HttpPost]
  public async Task<IActionResult> CreateCategory(CategoryRequest request)
  {
    await validator.ValidateAndThrowAsync(request);
    CategoryResponse createdCategory = await service.Create(request);
    return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
  }

  [HttpPut("{id:guid}")]
  public async Task<IActionResult> UpdateCategory(Guid id, CategoryRequest request)
  {
    await validator.ValidateAndThrowAsync(request);
    await service.Update(id, request);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> DeleteCategory(Guid id)
  {
    await service.Delete(id);
    return NoContent();
  }
}