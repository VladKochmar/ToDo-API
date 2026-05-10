using FluentValidation;
using ToDoApi.Models.DTOs;

namespace ToDoApi.Validations;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
  public CreateTaskRequestValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty()
      .MaximumLength(255);

    RuleFor(x => x.Description)
      .MaximumLength(500)
      .When(x => x.Description is not null);
    
    RuleFor(x => x.DueDate)
      .GreaterThan(DateTimeOffset.UtcNow)
      .When(x => x.DueDate.HasValue);
  }
}