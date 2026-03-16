using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Teachers.Commands.ReviewStudentAccess;

public class ReviewStudentAccessCommandHandler : IRequestHandler<ReviewStudentAccessCommand, Result<StudentTeacherConnectionDto>>
{
    private readonly IApplicationDbContext _context;

    public ReviewStudentAccessCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StudentTeacherConnectionDto>> Handle(ReviewStudentAccessCommand request, CancellationToken cancellationToken)
    {
        var connection = await _context.StudentTeacherConnections
            .FirstOrDefaultAsync(
                c => c.TeacherUserId == request.TeacherUserId && c.StudentUserId == request.StudentUserId,
                cancellationToken);

        if (connection == null)
        {
            return Result<StudentTeacherConnectionDto>.Failure("Student access request not found");
        }

        var teacherProfile = await _context.TeacherProfiles
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.UserId == request.TeacherUserId, cancellationToken);

        if (teacherProfile == null)
        {
            return Result<StudentTeacherConnectionDto>.Failure("Teacher profile not found");
        }

        connection.Status = request.Status;
        connection.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<StudentTeacherConnectionDto>.Success(new StudentTeacherConnectionDto
        {
            TeacherUserId = teacherProfile.UserId,
            TeacherCode = teacherProfile.TeacherCode ?? string.Empty,
            TeacherName = teacherProfile.FullName,
            SubjectName = teacherProfile.Subject?.Name ?? string.Empty,
            ConnectedAt = connection.ConnectedAt,
            Status = connection.Status.ToString(),
            ReviewedAt = connection.ReviewedAt,
        });
    }
}