using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Teachers.Queries.GetStudentAccessRequests;

public class GetStudentAccessRequestsQueryHandler : IRequestHandler<GetStudentAccessRequestsQuery, Result<List<StudentAccessRequestDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentAccessRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StudentAccessRequestDto>>> Handle(GetStudentAccessRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StudentTeacherConnections
            .Where(c => c.TeacherUserId == request.TeacherUserId)
            .Join(
                _context.StudentProfiles,
                c => c.StudentUserId,
                s => s.UserId,
                (c, s) => new { Connection = c, StudentProfile = s })
            .Join(
                _context.Users,
                x => x.StudentProfile.UserId,
                u => u.Id,
                (x, u) => new { x.Connection, x.StudentProfile, StudentUser = u })
            .Join(
                _context.TeacherProfiles,
                x => x.Connection.TeacherUserId,
                t => t.UserId,
                (x, t) => new StudentAccessRequestDto
                {
                    StudentUserId = x.StudentProfile.UserId,
                    StudentName = x.StudentProfile.FullName,
                    StudentEmail = x.StudentUser.Email ?? string.Empty,
                    TeacherCode = t.TeacherCode ?? string.Empty,
                    RequestedAt = x.Connection.ConnectedAt,
                    Status = x.Connection.Status.ToString(),
                    ReviewedAt = x.Connection.ReviewedAt,
                });

        if (request.Status.HasValue)
        {
            var statusText = request.Status.Value.ToString();
            query = query.Where(x => x.Status == statusText);
        }

        var items = await query
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);

        return Result<List<StudentAccessRequestDto>>.Success(items);
    }
}