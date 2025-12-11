using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace AuthTemplate.Models.User;

public sealed record UserChangePasswordRequest
{
    [Required]
    [MinLength(8)]
    public string CurrentPassword { get; init; }

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; }
}

public sealed class UserChangePasswordRequestValidator
    : AbstractValidator<UserChangePasswordRequest>
{
    public UserChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword) // Consistent with Identity's rules
            .NotEmpty()
            .WithMessage("New password is required")
            .NotEqual(x => x.CurrentPassword, StringComparer.OrdinalIgnoreCase)
            .WithMessage("New password cannot be the same as the old one.")
            .MinimumLength(8)
            .WithMessage("New password must be at least 8 characters.")
            .Matches("[0-9]")
            .WithMessage("New password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("New password must contain at least one non-alphanumeric character.")
            .Matches("[a-z]")
            .WithMessage("New password must contain at least one lowercase letter.")
            .Matches("[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter.");
    }
}
