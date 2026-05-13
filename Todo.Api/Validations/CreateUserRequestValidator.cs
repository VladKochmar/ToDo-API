using FluentValidation;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Validations;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
  public CreateUserRequestValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(255);

    RuleFor(x => x.FullName)
      .NotEmpty()
      .MaximumLength(255);
  }
}