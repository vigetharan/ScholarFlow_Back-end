using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Teachers.Queries.GetApprovalRequests;

public class GetTeacherApprovalRequestsQueryHandler : IRequestHandler<GetTeacherApprovalRequestsQuery, Result<List<TeacherApprovalRequestDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherApprovalRequestsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TeacherApprovalRequestDto>>> Handle(GetTeacherApprovalRequestsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TeacherProfiles
            .Include(t => t.User)
            .Include(t => t.Subject)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(t => t.Status == request.Status.Value);
        }

        var items = await query
            .OrderBy(t => t.Status)
            .ThenBy(t => t.FullName)
            .Select(t => new TeacherApprovalRequestDto
            {
                UserId = t.UserId,
                ProfileId = t.Id,
                Email = t.User.Email ?? string.Empty,
                FullName = t.FullName,
                Qualification = t.Qualification,
                SubjectId = t.SubjectId,
                SubjectName = t.Subject.Name,
                PhoneNumber = t.PhoneNumber,
                Status = t.Status.ToString(),
                TeacherCode = t.TeacherCode,
                PaperCount = _context.Papers.Count(p => p.CreatedByTeacher == t.UserId),
                RejectionReason = t.RejectionReason,
                ReviewedAt = t.ReviewedAt
            })
            .ToListAsync(cancellationToken);

        return Result<List<TeacherApprovalRequestDto>>.Success(items);
    }
}
