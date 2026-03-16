using FluentValidation;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Application.Features.Teachers.Commands.ReviewStudentAccess;

public class ReviewStudentAccessCommandValidator : AbstractValidator<ReviewStudentAccessCommand>
{
    public ReviewStudentAccessCommandValidator()
    {
        RuleFor(x => x.TeacherUserId)
            .NotEmpty().WithMessage("Teacher user id is required");

        RuleFor(x => x.StudentUserId)
            .NotEmpty().WithMessage("Student user id is required");

        RuleFor(x => x.Status)
            .Must(s => s == StudentTeacherConnectionStatus.Approved || s == StudentTeacherConnectionStatus.Rejected)
            .WithMessage("Status must be Approved or Rejected");
    }
}