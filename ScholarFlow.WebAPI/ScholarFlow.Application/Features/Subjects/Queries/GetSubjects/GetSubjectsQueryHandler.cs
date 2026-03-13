using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Subjects.Queries.GetSubjects;

/// <summary>
/// Handler for GetSubjectsQuery
/// </summary>
public class GetSubjectsQueryHandler : IRequestHandler<GetSubjectsQuery, Result<List<SubjectDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetSubjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<SubjectDto>>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
    {
        // Query subjects with their stream relationships
        var query = _context.Subjects
            .Include(s => s.StreamSubjects)
                .ThenInclude(ss => ss.Stream)
            .AsQueryable();

        // Filter by stream if provided
        if (request.StreamId.HasValue)
        {
            query = query.Where(s => s.StreamSubjects.Any(ss => ss.StreamId == request.StreamId.Value));
        }

        // If this is a student request, restrict subjects to selected subjects in the profile.
        if (request.StudentUserId.HasValue)
        {
            var profile = await _context.StudentProfiles
                .FirstOrDefaultAsync(p => p.UserId == request.StudentUserId.Value, cancellationToken);

            if (profile != null)
            {
                var selectedSubjectIds = await _context.StudentSubjectSelections
                    .Where(ss => ss.StudentProfileId == profile.Id)
                    .Select(ss => ss.SubjectId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                query = query.Where(s => selectedSubjectIds.Contains(s.Id));
            }
        }

        var subjects = await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        // Get non-deleted topic counts for all subjects in a single query
        var subjectIds = subjects.Select(s => s.Id).ToList();
        var counts = await _context.Topics
            .Where(t => subjectIds.Contains(t.SubjectId) && !t.IsDeleted)
            .GroupBy(t => t.SubjectId)
            .Select(g => new { SubjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SubjectId, x => x.Count, cancellationToken);

        // Map to DTOs
        var dtos = subjects.Select(s => new SubjectDto
        {
            Id = s.Id,
            Name = s.Name,
            StreamIds = s.StreamSubjects.Select(ss => ss.StreamId).ToList(),
            StreamNames = s.StreamSubjects.Select(ss => ss.Stream.Name).ToList(),
            TopicCount = counts.ContainsKey(s.Id) ? counts[s.Id] : 0
        }).ToList();

        return Result<List<SubjectDto>>.Success(dtos);
    }
}
