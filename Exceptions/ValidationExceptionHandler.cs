using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ToDoApi.Exceptions;
/// <summary>
/// Validation exception handler (FluentValidation).
/// </summary>
/// <remarks>
/// Catches <see cref="FluentValidation.ValidationException"/>, sets the status to 400 Bad Request, 
/// and returns error details in Problem Details format.
/// </remarks>
public sealed class ValidationExceptionHandler(
  IProblemDetailsService problemDetailsService,
  ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
  /// <summary>
  /// Attempt to handle an exception if it is a validation error.
  /// </summary>
  /// <param name="httpContext">The context of the current HTTP request.</param>
  /// <param name="exception">An exception occurred during query execution.</param>
  /// <param name="cancellationToken">Transaction cancellation token.</param>
  /// <returns>
  /// <c>true</c> if the exception was handled (this is a ValidationException);
  /// otherwise <c>false</c> to pass processing to the next Handler.
  /// </returns>
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext, 
    Exception exception, 
    CancellationToken cancellationToken)
  {
    if (exception is not FluentValidation.ValidationException validationException)
      return false;

    logger.LogError(exception, "Unhandled exception occured");

    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
    ProblemDetailsContext context = new ()
    {
      HttpContext = httpContext,
      Exception = exception,
      ProblemDetails = new ProblemDetails
      {
        Detail = "One or more validation errors occured",
        Status = StatusCodes.Status400BadRequest
      }
    };

    Dictionary<string, string[]> errors = validationException.Errors
      .GroupBy(e => e.PropertyName)
      .ToDictionary(
        g => g.Key.ToLowerInvariant(),
        g => g.Select(e => e.ErrorMessage).ToArray()
      );
    
    context.ProblemDetails.Extensions.Add("errors", errors);

    return await problemDetailsService.TryWriteAsync(context);
  }
}