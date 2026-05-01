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
  public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories() {
    IReadOnlyList<CategoryResponse> response = await service.GetAll();
    return Ok(response);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<CategoryResponse>> GetCategoryById(Guid id)
  {
    CategoryResponse response = await service.GetById(id);
    return Ok(response);
  }

  [HttpPost]
  public async Task<ActionResult<CategoryResponse>> CreateCategory(CategoryRequest request)
  {
    await validator.ValidateAndThrowAsync(request);
    CategoryResponse createdCategory = await service.Create(request);
    return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> UpdateCategory(Guid id, CategoryRequest request)
  {
    await validator.ValidateAndThrowAsync(request);
    await service.Update(id, request);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> DeleteCategory(Guid id)
  {
    await service.Delete(id);
    return NoContent();
  }
}