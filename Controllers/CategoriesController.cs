using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models.DTOs;
using ToDoApi.Services;

namespace ToDoApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController(
  ICategoryService service, 
  IUserContext userContext, 
  IValidator<CategoryRequest> validator
) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories() {
    Guid userId = await userContext.UserId();
    IReadOnlyList<CategoryResponse> response = await service.GetAll(userId);
    return Ok(response);
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<CategoryResponse>> GetCategoryById(Guid id)
  {
    Guid userId = await userContext.UserId();
    CategoryResponse response = await service.GetById(id, userId);
    return Ok(response);
  }

  [HttpPost]
  public async Task<ActionResult<CategoryResponse>> CreateCategory(CategoryRequest request)
  {
    Guid userId = await userContext.UserId();
    await validator.ValidateAndThrowAsync(request);
    CategoryResponse createdCategory = await service.Create(userId, request);
    return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult> UpdateCategory(Guid id, CategoryRequest request)
  {
    Guid userId = await userContext.UserId();
    await validator.ValidateAndThrowAsync(request);
    await service.Update(id, userId, request);
    return NoContent();
  }

  [HttpDelete("{id:guid}")]
  public async Task<ActionResult> DeleteCategory(Guid id)
  {
    Guid userId = await userContext.UserId();
    await service.Delete(id, userId);
    return NoContent();
  }
}