using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Questions.Queries.GetAllQuestions;

/// <summary>
/// Returns all questions for the question bank management view
/// </summary>
public class GetAllQuestionsQueryHandler
    : IRequestHandler<GetAllQuestionsQuery, Result<List<QuestionDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAllQuestionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<QuestionDto>>> Handle(
        GetAllQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var questions = await _context.Questions
            .Include(q => q.SubTopic)
                .ThenInclude(st => st.Topic)
                    .ThenInclude(t => t.Subject)
                        .ThenInclude(s => s.StreamSubjects)
                            .ThenInclude(ss => ss.Stream)
            .Include(q => q.Options)
            .Include(q => q.Explanations)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

        var dtos = questions.Select(q => new QuestionDto
        {
            Id               = q.Id,
            PaperId          = q.PaperId,
            SubTopicId       = q.SubTopicId,
            SubTopicName     = q.SubTopic?.SubTopicName                       ?? string.Empty,
            TopicName        = q.SubTopic?.Topic?.TopicName                   ?? string.Empty,
            SubjectName      = q.SubTopic?.Topic?.Subject?.Name               ?? string.Empty,
            StreamName       = q.SubTopic?.Topic?.Subject?.StreamSubjects
                                    .Select(ss => ss.Stream.Name).FirstOrDefault() ?? string.Empty,
            QuestionText     = q.QuestionText,
            QuestionImageUrl = q.QuestionImageUrl,
            Difficulty       = q.Difficulty,
            Marks            = q.Marks,
            OrderIndex       = q.OrderIndex,
            BloomLevel       = q.BloomLevel,
            ReviewStatus     = q.ReviewStatus,
            Tags             = q.Tags
                                 .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(t => t.Trim())
                                 .ToList(),
            IsFlagged        = q.IsFlagged,
            FlagReason       = q.FlagReason,
            UsageCount       = q.UsageCount,
            SuccessRate      = q.SuccessRate,
            Options          = q.Options.Select(o => new OptionDto
            {
                Id         = o.Id,
                OptionText = o.OptionText,
                IsCorrect  = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).OrderBy(o => o.OrderIndex).ToList(),
            ReviewMaterials  = new List<QuestionReviewDto>() // loaded separately if needed
        }).ToList();

        return Result<List<QuestionDto>>.Success(dtos);
    }
}