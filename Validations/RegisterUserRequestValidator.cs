using FluentValidation;
using ToDoApi.Models.DTOs;

namespace ToDoApi.Validations;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
  public RegisterUserRequestValidator()
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

    RuleFor(x => x.Password)
      .NotEmpty()
      .MinimumLength(8)
      .MaximumLength(50)
      .Matches("[A-Z]").WithMessage("At least one uppercase letter required")
      .Matches("[a-z]").WithMessage("At least one lowercase letter required")
      .Matches("[0-9]").WithMessage("At least one digit required");
  }
}