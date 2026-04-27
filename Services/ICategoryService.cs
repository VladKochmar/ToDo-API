using ToDoApi.Models.DTOs;

namespace ToDoApi.Services;

public interface ICategoryService
{
  /// <summary>
  /// Gets the details of a specific category by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the category to be found.</param>
  /// <returns>
  /// A <see cref="CategoryResponse"/> object with the identifier and title of the found category.
  /// </returns>
  /// <exception cref="NotFoundException">
  /// Occurs if the category with the specified <paramref name="id"/> is not found
  /// or does not belong to the currently authorized user.
  /// </exception>
  public Task<CategoryResponse> GetById(Guid id);

  /// <summary>
  /// Gets a list of all categories owned by the currently logged in user.
  /// </summary>
  /// <returns>
  /// A read-only list of <see cref="CategoryResponse"/> objects. 
  /// If there are no categories, an empty list is returned.
  /// </returns>
  public Task<IReadOnlyList<CategoryResponse>> GetAll();

  /// <summary>
  /// Creates a new category for the current user.
  /// </summary>
  /// <param name="request">An object with new data for a category (Title).</param>
  /// <returns>
  /// A <see cref="CategoryResponse"/> object with the identifier and title of the created category.
  /// </returns>
  /// <exception cref="ConflictException">
  /// Occurs if the user already has another category with the same title as in <paramref name="request"/>.
  /// </exception>
  public Task<CategoryResponse> Create(CategoryRequest request);
  
  /// <summary>
  /// Updates a category by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the category to be updated.</param>
  /// <param name="request">An object with new data for a category (Title).</param>
  /// <returns>Task</returns>
  /// <exception cref="NotFoundException">
  /// Occurs if the category with the specified <paramref name="id"/> is not found
  /// or does not belong to the currently authorized user.
  /// </exception>
  /// <exception cref="ConflictException">
  /// Occurs if the user already has another category with the same title as in <paramref name="request"/>.
  /// </exception>
  public Task Update(Guid id, CategoryRequest request);
  
  /// <summary>
  /// Deletes a category by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the category to be deleted.</param>
  /// <returns>Task</returns>
  /// <exception cref="NotFoundException">
  /// Occurs if the category with the specified <paramref name="id"/> is not found 
  /// or does not belong to the currently authorized user.
  /// </exception>
  public Task Delete(Guid id);
}