using FluentValidation;

namespace AuthTemplate.Models.User;

public sealed record UserUpdateRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? DateOfBirthUtc { get; init; }
}

public sealed class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MinimumLength(2)
            .WithMessage("First Name must be at least 2 characters long");

        RuleFor(x => x.LastName)
            .MinimumLength(2)
            .WithMessage("Last Name must be at least 2 characters long");

        RuleFor(x => x.DateOfBirthUtc)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DateOfBirthUtc.HasValue)
            .WithMessage("Date of birth must be in the past");
    }
}
