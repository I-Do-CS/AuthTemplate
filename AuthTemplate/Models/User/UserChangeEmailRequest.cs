using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace AuthTemplate.Models.User;

public sealed record UserChangeEmailRequest
{
    [EmailAddress]
    [Required]
    public string NewEmail { get; init; }
}

public sealed class UserChangeEmailRequestValidator : AbstractValidator<UserChangeEmailRequest>
{
    public UserChangeEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .WithMessage("New Email is required")
            .EmailAddress()
            .WithMessage("Invalid email");
    }
}
