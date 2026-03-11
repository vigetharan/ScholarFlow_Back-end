using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Topics.Queries.GetTopicById;

/// <summary>
/// Handler for GetTopicByIdQuery
/// </summary>
public class GetTopicByIdQueryHandler : IRequestHandler<GetTopicByIdQuery, Result<TopicDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTopicByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TopicDto>> Handle(GetTopicByIdQuery request, CancellationToken cancellationToken)
    {
        // Get topic with subject
        var topic = await _context.Topics
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (topic == null)
        {
            return Result<TopicDto>.Failure("Topic not found");
        }

        // Count non-deleted subtopics for this topic
        var subtopicCount = await _context.SubTopics
            .CountAsync(st => st.TopicId == topic.Id && !st.IsDeleted, cancellationToken);

        // Map to DTO
        var dto = new TopicDto
        {
            Id = topic.Id,
            TopicName = topic.TopicName,
            SubjectId = topic.SubjectId,
            SubjectName = topic.Subject?.Name ?? "",
            SubTopicCount = subtopicCount
        };

        return Result<TopicDto>.Success(dto);
    }
}
