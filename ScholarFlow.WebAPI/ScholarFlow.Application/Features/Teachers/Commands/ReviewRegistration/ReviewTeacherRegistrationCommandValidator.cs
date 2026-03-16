using FluentValidation;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Application.Features.Teachers.Commands.ReviewRegistration;

public class ReviewTeacherRegistrationCommandValidator : AbstractValidator<ReviewTeacherRegistrationCommand>
{
    public ReviewTeacherRegistrationCommandValidator()
    {
        RuleFor(x => x.TeacherUserId)
            .NotEmpty().WithMessage("Teacher user id is required");

        RuleFor(x => x.ReviewedById)
            .NotEmpty().WithMessage("Reviewer id is required");

        RuleFor(x => x.Status)
            .Must(s => s == TeacherRegistrationStatus.Accepted || s == TeacherRegistrationStatus.Rejected)
            .WithMessage("Status must be Accepted or Rejected");

        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required when status is Rejected")
            .MaximumLength(500)
            .When(x => x.Status == TeacherRegistrationStatus.Rejected);
    }
}
