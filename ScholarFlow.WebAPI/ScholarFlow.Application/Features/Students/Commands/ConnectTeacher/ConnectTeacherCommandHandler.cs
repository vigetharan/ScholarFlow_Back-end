using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Enums;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Students.Commands.ConnectTeacher;

public class ConnectTeacherCommandHandler : IRequestHandler<ConnectTeacherCommand, Result<StudentTeacherConnectionDto>>
{
    private readonly IApplicationDbContext _context;

    public ConnectTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StudentTeacherConnectionDto>> Handle(ConnectTeacherCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.TeacherCode.Trim().ToUpperInvariant();

        var teacherProfile = await _context.TeacherProfiles
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(
                t => t.TeacherCode == normalizedCode && t.Status == TeacherRegistrationStatus.Accepted,
                cancellationToken);

        if (teacherProfile == null)
        {
            return Result<StudentTeacherConnectionDto>.Failure("Invalid teacher code");
        }

        var studentExists = await _context.StudentProfiles
            .AnyAsync(s => s.UserId == request.StudentUserId, cancellationToken);

        if (!studentExists)
        {
            return Result<StudentTeacherConnectionDto>.Failure("Student profile not found");
        }

        var connection = await _context.StudentTeacherConnections
            .FirstOrDefaultAsync(
                c => c.StudentUserId == request.StudentUserId && c.TeacherUserId == teacherProfile.UserId,
                cancellationToken);

        if (connection == null)
        {
            connection = new StudentTeacherConnection
            {
                Id = Guid.NewGuid(),
                StudentUserId = request.StudentUserId,
                TeacherUserId = teacherProfile.UserId,
                ConnectedAt = DateTime.UtcNow,
                Status = StudentTeacherConnectionStatus.Pending,
            };

            _context.StudentTeacherConnections.Add(connection);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else if (connection.Status == StudentTeacherConnectionStatus.Rejected)
        {
            connection.Status = StudentTeacherConnectionStatus.Pending;
            connection.ConnectedAt = DateTime.UtcNow;
            connection.ReviewedAt = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

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
