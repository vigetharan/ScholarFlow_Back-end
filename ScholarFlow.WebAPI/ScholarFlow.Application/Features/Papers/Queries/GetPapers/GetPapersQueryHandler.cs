using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Papers.Queries.GetPapers;

public class GetPapersQueryHandler : IRequestHandler<GetPapersQuery, Result<List<PaperDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetPapersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PaperDto>>> Handle(GetPapersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Papers
            .Include(p => p.Subject)
            .Include(p => p.Creator)
            .Include(p => p.Questions)
            .AsQueryable();

        // Apply filters
        if (request.SubjectId.HasValue)
        {
            query = query.Where(p => p.SubjectId == request.SubjectId.Value);
        }

        if (request.Year.HasValue)
        {
            query = query.Where(p => p.Year == request.Year.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(p => p.Type == request.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.TeacherCode))
        {
            var normalizedCode = request.TeacherCode.Trim().ToUpperInvariant();
            var teacherUserId = await _context.TeacherProfiles
                .Where(t => t.TeacherCode == normalizedCode && t.Status == TeacherRegistrationStatus.Accepted)
                .Select(t => (Guid?)t.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!teacherUserId.HasValue)
            {
                return Result<List<PaperDto>>.Success(new List<PaperDto>());
            }

            if (!request.StudentUserId.HasValue)
            {
                return Result<List<PaperDto>>.Success(new List<PaperDto>());
            }

            var hasApprovedAccess = await _context.StudentTeacherConnections
                .AnyAsync(
                    c => c.StudentUserId == request.StudentUserId.Value
                         && c.TeacherUserId == teacherUserId.Value
                         && c.Status == StudentTeacherConnectionStatus.Approved,
                    cancellationToken);

            if (!hasApprovedAccess)
            {
                return Result<List<PaperDto>>.Success(new List<PaperDto>());
            }

            query = query.Where(p => p.CreatedByTeacher == teacherUserId.Value);
        }

        var dtos = await query
            .OrderByDescending(p => p.Year)
            .ThenBy(p => p.Subject.Name)
            .Select(p => new PaperDto
            {
                Id = p.Id,
                SubjectId = p.SubjectId,
                SubjectName = p.Subject.Name,
                Year = p.Year,
                Type = p.Type.ToString(),
                Title = p.Title,
                TimeLimit = p.TimeLimit,
                CreatedByTeacher = p.CreatedByTeacher,
                CreatedByTeacherName = p.Creator.UserName ?? "",
                QuestionCount = p.Questions.Count
            })
            .ToListAsync(cancellationToken);

        return Result<List<PaperDto>>.Success(dtos);
    }
}
