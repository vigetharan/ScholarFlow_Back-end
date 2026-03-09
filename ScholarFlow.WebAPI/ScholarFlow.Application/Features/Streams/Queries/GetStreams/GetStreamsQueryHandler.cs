using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Streams.Queries.GetStreams;

/// <summary>
/// Handler for GetStreamsQuery
/// </summary>
public class GetStreamsQueryHandler : IRequestHandler<GetStreamsQuery, Result<List<StreamDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetStreamsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<StreamDto>>> Handle(GetStreamsQuery request, CancellationToken cancellationToken)
    {
        var streams = await _context.Streams
            .Include(s => s.StreamSubjects)
                .ThenInclude(ss => ss.Subject)
            .OrderBy(s => s.Name)
            .Select(s => new StreamDto
            {
                Id = s.Id,
                Name = s.Name,
                CreatedAt = s.CreatedAt,
                SubjectIds = s.StreamSubjects.Select(ss => ss.SubjectId).ToList(),
                SubjectNames = s.StreamSubjects.Select(ss => ss.Subject.Name).ToList()
            })
            .ToListAsync(cancellationToken);

        return Result<List<StreamDto>>.Success(streams);
    }
}
