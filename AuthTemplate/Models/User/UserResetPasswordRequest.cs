using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace AuthTemplate.Models.User;

public sealed record UserResetPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string NewPassword { get; init; }
}

public sealed class UserResetPasswordRequestValidator : AbstractValidator<UserResetPasswordRequest>
{
    public UserResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email");

        RuleFor(x => x.NewPassword) // Consistent with Identity's rules
            .NotEmpty()
            .WithMessage("New password is required")
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
