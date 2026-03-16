using FluentValidation;

namespace ScholarFlow.Application.Features.Auth.Commands.Register;

/// <summary>
/// Validator for RegisterCommand
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");

        RuleFor(x => x.Role)
            .Must(role => new[] { "Admin", "Teacher", "Student" }.Contains(role))
            .WithMessage("Role must be Admin, Teacher, or Student");

        When(x => string.Equals(x.Role, "Teacher", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required for teacher registration")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

            RuleFor(x => x.Qualification)
                .NotEmpty().WithMessage("Qualification is required for teacher registration")
                .MaximumLength(200).WithMessage("Qualification must not exceed 200 characters");

            RuleFor(x => x.SubjectId)
                .NotEmpty().WithMessage("Subject is required for teacher registration");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required for teacher registration")
                .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");
        });
    }
}
