using FluentValidation;

namespace HabitDev.DTOs.Tags;

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(3)
            .WithMessage($"a name cannot be empty");

        RuleFor(x => x.Description)
            .MaximumLength(50);
    }
}
