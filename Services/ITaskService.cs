namespace ToDoApi.Models.DTOs;

public interface ITaskService
{
  /// <summary>
  /// Gets the details of a specific task by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the task to be found.</param>
  /// <returns>
  /// A <see cref="TaskResponse"/> object with the identifier and data of the found task.
  /// </returns>
  /// <exception cref="NotFoundException">
  /// Occurs if the task with the specified <paramref name="id"/> is not found
  /// or does not belong to the currently authorized user.
  /// </exception>
  public Task<TaskResponse> GetById(Guid id);

  /// <summary>
  /// Gets a list of all tasks owned by the currently logged in user.
  /// </summary>
  /// <returns>
  /// A read-only list of <see cref="TaskResponse"/> objects. 
  /// If there are no tasks, an empty list is returned.
  /// </returns>
  public Task<IReadOnlyList<TaskResponse>> GetAll();

  /// <summary>
  /// Creates a new task for the current user.
  /// </summary>
  /// <param name="request">An object with new data for a task (Title, Description, DueDate, CategoryId).</param>
  /// <returns>
  /// A <see cref="TaskResponse"/> object with the identifier and data of the created task.
  /// </returns>
  /// <exception cref="NotFoundException">
  /// Occurs if <see cref="CreateTaskRequest.CategoryId"/> is specified, but such a category does not exist 
  /// or does not belong to the current user.
  /// </exception>
  /// <exception cref="ConflictException">
  /// Occurs if the user already has a task with the same title within the selected category 
  /// (or without a category if no ID is specified).
  /// </exception>
  public Task<TaskResponse> Create(CreateTaskRequest request);

  /// <summary>
  /// Updates the data of an existing user task by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the task to be updated.</param>
  /// <param name="request">An object with new data for a task (Title, Description, IsCompleted, DueDate, CategoryId).</param>
  /// <returns>Task</returns>
  /// <exception cref="NotFoundException">
  /// It occurs in two cases:
  /// 1. The task with the specified <paramref name="id"/> was not found or does not belong to the user.
  /// 2. The category specified in the request (<see cref="UpdateTaskRequest.CategoryId"/>) does not exist.
  /// </exception>
  /// <exception cref="ConflictException">
  /// It occurs in two cases:
  /// 1. The task already has the status "completed" (<see cref="TaskItem.IsCompleted"/>), which prevents further editing.
  /// 2. The user already has another task with the same title within the same category.
  /// </exception>
  public Task Update(Guid id, UpdateTaskRequest request);

  /// <summary>
  /// Deletes a task by its ID.
  /// </summary>
  /// <param name="id">The unique identifier (GUID) of the task to be deleted.</param>
  /// <returns>Task</returns>
  /// <exception cref="NotFoundException">
  /// Occurs if the task with the specified <paramref name="id"/> is not found 
  /// or does not belong to the currently authorized user.
  /// </exception>
  public Task Delete(Guid id);
}