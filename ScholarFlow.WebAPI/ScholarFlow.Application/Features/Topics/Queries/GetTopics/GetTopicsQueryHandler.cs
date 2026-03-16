using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Topics.Queries.GetTopics;

/// <summary>
/// Handler for GetTopicsQuery
/// </summary>
public class GetTopicsQueryHandler : IRequestHandler<GetTopicsQuery, Result<List<TopicDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetTopicsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<TopicDto>>> Handle(GetTopicsQuery request, CancellationToken cancellationToken)
    {
        // Query topics with subject, filter out deleted ones
        var query = _context.Topics
            .Include(t => t.Subject)
            .Where(t => !t.IsDeleted) // Only get non-deleted topics
            .AsQueryable();

        // Filter by subject if provided
        if (request.SubjectId.HasValue)
        {
            query = query.Where(t => t.SubjectId == request.SubjectId.Value);
        }

        var topics = await query
            .OrderBy(t => t.OrderIndex)
            .ThenBy(t => t.TopicName)
            .ToListAsync(cancellationToken);

        // Aggregate non-deleted subtopic counts per topic to avoid N+1 queries
        var topicIds = topics.Select(t => t.Id).ToList();
        var subtopicCounts = await _context.SubTopics
            .Where(st => topicIds.Contains(st.TopicId) && !st.IsDeleted)
            .GroupBy(st => st.TopicId)
            .Select(g => new { TopicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TopicId, x => x.Count, cancellationToken);

        // Map to DTOs
        var dtos = topics.Select(t => new TopicDto
        {
            Id = t.Id,
            TopicName = t.TopicName,
            OrderIndex = t.OrderIndex,
            SubjectId = t.SubjectId,
            SubjectName = t.Subject?.Name ?? "",
            SubTopicCount = subtopicCounts.ContainsKey(t.Id) ? subtopicCounts[t.Id] : 0
        }).ToList();

        return Result<List<TopicDto>>.Success(dtos);
    }
}
