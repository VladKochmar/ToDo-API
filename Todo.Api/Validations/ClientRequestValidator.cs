using FluentValidation;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Validations;

public sealed class ClientRequestValidator : AbstractValidator<ClientRequest>
{
  public ClientRequestValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .MaximumLength(100);
  }
}