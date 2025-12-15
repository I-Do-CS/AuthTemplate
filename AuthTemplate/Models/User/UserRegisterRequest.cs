using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace AuthTemplate.Models.User;

public sealed record UserRegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; init; }

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; init; }

    [Required]
    [MaxLength(50)]
    [EmailAddress]
    public string Email { get; init; }

    [Required]
    [MinLength(8)]
    public string Password { get; init; }

    public DateOnly? DateOfBirthUtc { get; init; }
}

public sealed class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name is required");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Email is not valid");

        RuleFor(x => x.Password) // Consistent with Identity's rules
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one non-alphanumeric character.")
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.");

        RuleFor(x => x.DateOfBirthUtc)
            .Must(d => d >= DateOnly.FromDateTime(DateTime.Today))
            .When(x => x.DateOfBirthUtc.HasValue)
            .WithMessage("Date of birth must be in the past");
    }
}
