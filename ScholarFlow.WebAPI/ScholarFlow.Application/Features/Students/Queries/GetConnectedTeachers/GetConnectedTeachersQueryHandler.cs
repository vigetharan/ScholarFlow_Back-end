using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Students.Queries.GetConnectedTeachers;

public class GetConnectedTeachersQueryHandler : IRequestHandler<GetConnectedTeachersQuery, Result<List<StudentTeacherConnectionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetConnectedTeachersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StudentTeacherConnectionDto>>> Handle(GetConnectedTeachersQuery request, CancellationToken cancellationToken)
    {
        var data = await _context.StudentTeacherConnections
            .Where(c => c.StudentUserId == request.StudentUserId)
            .Join(
                _context.TeacherProfiles.Include(t => t.Subject).Where(t => t.Status == TeacherRegistrationStatus.Accepted),
                c => c.TeacherUserId,
                t => t.UserId,
                (c, t) => new StudentTeacherConnectionDto
                {
                    TeacherUserId = t.UserId,
                    TeacherCode = t.TeacherCode ?? string.Empty,
                    TeacherName = t.FullName,
                    SubjectName = t.Subject != null ? t.Subject.Name : string.Empty,
                    ConnectedAt = c.ConnectedAt,
                    Status = c.Status.ToString(),
                    ReviewedAt = c.ReviewedAt,
                })
            .OrderBy(x => x.TeacherName)
            .ToListAsync(cancellationToken);

        return Result<List<StudentTeacherConnectionDto>>.Success(data);
    }
}
