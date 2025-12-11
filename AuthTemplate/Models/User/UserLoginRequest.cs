using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace AuthTemplate.Models.User;

public sealed record UserLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; }

    [Required]
    [MinLength(8)]
    public string Password { get; init; }
}

public sealed class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Emali is required")
            .EmailAddress()
            .WithMessage("Email is invalid");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("The Password must be at least 8 characters long");
    }
}
