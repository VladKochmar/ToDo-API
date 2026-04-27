using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ToDoApi.Exceptions;

/// <summary>
/// Global exception handler for the entire application.
/// </summary>
/// <remarks>
/// This class catches all unhandled exceptions and converts them into a structured Problem Details response.
/// It is responsible for mapping domain exceptions (NotFoundException, ConflictException, etc.) to corresponding HTTP codes.
/// </remarks>
public sealed class GlobalExceptionHandler(
  IProblemDetailsService problemDetailsService,
  ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
  /// <summary>
  /// Converts the exception into a corresponding HTTP status and JSON response.
  /// </summary>
  /// <param name="httpContext">The context of the current HTTP request.</param>
  /// <param name="exception">An exception occurred during query execution.</param>
  /// <param name="cancellationToken">Transaction cancellation token.</param>
  /// <returns>Always returns <c>true</c> as this is the final handler that ends the query loop.</returns>
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext, 
    Exception exception, 
    CancellationToken cancellationToken)
  {
    logger.LogError(exception, "Unhandled exception occured");

    httpContext.Response.StatusCode = exception switch
    {
      NotFoundException => StatusCodes.Status404NotFound,
      ArgumentException => StatusCodes.Status400BadRequest,
      ConflictException => StatusCodes.Status409Conflict,
      _ => StatusCodes.Status500InternalServerError
    };

    return await problemDetailsService.TryWriteAsync(
    new ProblemDetailsContext
    {
      HttpContext = httpContext,
      Exception = exception,
      ProblemDetails = new ProblemDetails
      {
        Type = exception.GetType().Name,
        Title = "An error occured",
        Detail = exception.Message
      }
    });
  }
}