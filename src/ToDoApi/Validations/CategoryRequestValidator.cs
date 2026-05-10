using FluentValidation;
using ToDoApi.Models.DTOs;

namespace ToDoApi.Validations;

public sealed class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
  public CategoryRequestValidator()
  {
    RuleFor(x => x.Title)
      .NotEmpty()
      .MaximumLength(100);
  }
}