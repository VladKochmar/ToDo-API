using FluentValidation;
using ToDoApi.Models.DTOs;

namespace ToDoApi.Validations;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
  public CreateUserRequestValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(255);

    RuleFor(x => x.FirstName)
      .NotEmpty()
      .MaximumLength(100);

    RuleFor(x => x.LastName)
      .NotEmpty()
      .MaximumLength(100);
  }
}