namespace Todo.Api.Services;
public interface IUserContext
{
  /// <summary>
  /// Gets internal application user identifier (UUID) associated with the currently authorized user.
  /// </summary>
  /// <returns>
  /// A <see cref="Guid"/> representing the application's internal user ID.
  /// </returns>
  /// <exception cref="UnauthorizedAccessException">
  /// Thrown when no matching user is found in the database for the current authentication context.
  /// </exception>
  Task<Guid> UserId();
};