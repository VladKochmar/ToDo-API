using FluentValidation;
using Todo.Api.Models.DTOs;

namespace Todo.Api.Validations;

public sealed class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
  public CategoryRequestValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty()
      .MaximumLength(100);
  }
}