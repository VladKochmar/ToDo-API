namespace ToDoApi.Services;
public interface IUserContext
{
  /// <summary>
  /// Gets the unique identifier (Subject ID) of the currently authorized user.
  /// </summary>
  /// <returns>A string with the user ID from claims (NameIdentifier).</returns>
  /// <exception cref="UnauthorizedAccessException">
  /// Occurs if the user is not authorized or their token lacks the required claim <see cref="ClaimTypes.NameIdentifier"/>.
  /// </exception>
  string AuthId();
};