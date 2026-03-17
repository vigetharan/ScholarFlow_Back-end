using FluentValidation;

namespace ScholarFlow.Application.Features.Students.Commands.ConnectTeacher;

public class ConnectTeacherCommandValidator : AbstractValidator<ConnectTeacherCommand>
{
    public ConnectTeacherCommandValidator()
    {
        RuleFor(x => x.StudentUserId)
            .NotEmpty().WithMessage("Student user id is required");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject id is required");

        RuleFor(x => x.TeacherCode)
            .NotEmpty().WithMessage("Teacher code is required")
            .MaximumLength(20).WithMessage("Teacher code is too long");
    }
}
