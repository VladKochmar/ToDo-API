using FluentValidation;
using ToDoApi.Models.DTOs;

namespace ToDoApi.Validations;

public sealed class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
  public LoginUserRequestValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress();

    RuleFor(x => x.Password)
      .NotEmpty();
  }
}