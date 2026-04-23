using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ToDoApi.Exceptions;

public sealed class ValidationExceptionHandler(
  IProblemDetailsService problemDetailsService,
  ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
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