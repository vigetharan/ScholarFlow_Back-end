using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.SubTopics.Commands.CreateSubTopic;

public class CreateSubTopicCommandHandler : IRequestHandler<CreateSubTopicCommand, Result<SubTopicDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateSubTopicCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SubTopicDto>> Handle(CreateSubTopicCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.SubTopicName.Trim();

        var topic = await _context.Topics
            .FirstOrDefaultAsync(t => t.Id == request.TopicId, cancellationToken);

        if (topic == null)
        {
            return Result<SubTopicDto>.Failure("Topic not found");
        }

        var existingSubTopic = await _context.SubTopics
            .FirstOrDefaultAsync(st => st.TopicId == request.TopicId
                                      && st.SubTopicName == normalizedName,
                                cancellationToken);

        if (existingSubTopic != null)
        {
            return Result<SubTopicDto>.Failure($"SubTopic '{request.SubTopicName}' already exists in this topic");
        }

        var subTopic = new SubTopic
        {
            Id = Guid.NewGuid(),
            SubTopicName = normalizedName,
            TopicId = request.TopicId
        };

        _context.SubTopics.Add(subTopic);
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Result<SubTopicDto>.Failure($"SubTopic '{normalizedName}' already exists in this topic");
        }

        var dto = new SubTopicDto
        {
            Id = subTopic.Id,
            SubTopicName = subTopic.SubTopicName,
            TopicId = subTopic.TopicId,
            TopicName = topic.TopicName
        };

        return Result<SubTopicDto>.Success(dto);
    }
}
