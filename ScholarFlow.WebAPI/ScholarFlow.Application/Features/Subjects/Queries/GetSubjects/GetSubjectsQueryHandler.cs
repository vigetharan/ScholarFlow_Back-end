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
        // Query subjects with their streams
        var query = _context.Subjects
            .Include(s => s.Stream)
            .AsQueryable();

        // Filter by stream if provided
        if (request.StreamId.HasValue)
        {
            query = query.Where(s => s.StreamId == request.StreamId.Value);
        }

        var subjects = await query
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var dtos = subjects.Select(s => new SubjectDto
        {
            Id = s.Id,
            Name = s.Name,
            StreamId = s.StreamId,
            StreamName = s.Stream?.Name
        }).ToList();

        return Result<List<SubjectDto>>.Success(dtos);
    }
}
