using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Questions.Queries.GetQuestionsByTopic;

/// <summary>
/// Handler for GetQuestionsByTopicQuery.
/// </summary>
public class GetQuestionsByTopicQueryHandler : IRequestHandler<GetQuestionsByTopicQuery, Result<List<QuestionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetQuestionsByTopicQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<QuestionDto>>> Handle(GetQuestionsByTopicQuery request, CancellationToken cancellationToken)
    {
        var take = request.Limit.GetValueOrDefault(20);
        if (take <= 0)
        {
            take = 20;
        }

        if (take > 100)
        {
            take = 100;
        }

        var selectedIds = await _context.Questions
            .Where(q => q.SubTopic.TopicId == request.TopicId)
            .Select(q => q.Id)
            .Distinct()
            .OrderBy(_ => Guid.NewGuid())
            .Take(take)
            .ToListAsync(cancellationToken);

        if (selectedIds.Count == 0)
        {
            return Result<List<QuestionDto>>.Success(new List<QuestionDto>());
        }

        var questions = await _context.Questions
            .Include(q => q.SubTopic)
            .Include(q => q.Options)
            .Include(q => q.Explanations)
                .ThenInclude(e => e.Sections)
            .Where(q => selectedIds.Contains(q.Id))
            .ToListAsync(cancellationToken);

        var shuffled = questions
            .DistinctBy(q => q.Id)
            .OrderBy(_ => Guid.NewGuid())
            .ToList();

        var dtos = shuffled.Select(q => new QuestionDto
        {
            Id = q.Id,
            PaperId = q.PaperId,
            SubTopicId = q.SubTopicId,
            SubTopicName = q.SubTopic?.SubTopicName ?? string.Empty,
            QuestionText = q.QuestionText,
            QuestionImageUrl = q.QuestionImageUrl,
            Explanation = q.Explanations
                .SelectMany(e => e.Sections)
                .Where(s => s.Type == ContentType.Text || s.Type == ContentType.Formula || s.Type == ContentType.Code)
                .OrderBy(s => s.OrderIndex)
                .Select(s => s.Content)
                .FirstOrDefault() ?? q.Explanations.Select(e => e.Title).FirstOrDefault(),
            Difficulty = q.Difficulty,
            Marks = q.Marks,
            OrderIndex = q.OrderIndex,
            Options = q.Options.Select(o => new OptionDto
            {
                Id = o.Id,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).OrderBy(o => o.OrderIndex).ToList()
        }).ToList();

        return Result<List<QuestionDto>>.Success(dtos);
    }
}
