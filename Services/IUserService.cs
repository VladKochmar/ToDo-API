namespace ToDoApi.Services;

public interface IUserService
{
  /// <summary>
  /// Synchronizes user data with information from the ID token (OAuth2/OpenID Connect).
  /// </summary>
  /// <remarks>
  /// The method checks for the presence of a user by <c>sub</c> (Subject) from the token.
  /// If the user does not exist, it creates a new one, if it exists, it updates their data (Email, FirstName, LastName).
  /// </remarks>
  /// <param name="idToken">A string in JWT (ID Token) format containing user data.</param>
  /// <returns>Task</returns>
  /// <exception cref="ArgumentException">
  /// Occurs if the provided <paramref name="idToken"/> is not a valid JWT token.
  /// </exception>
  /// <exception cref="FluentValidation.ValidationException">
  /// Occurs if the data extracted from the token (Email, FirstName, LastName) did not pass validation.
  /// </exception>
  Task SyncUser(string idToken);
}