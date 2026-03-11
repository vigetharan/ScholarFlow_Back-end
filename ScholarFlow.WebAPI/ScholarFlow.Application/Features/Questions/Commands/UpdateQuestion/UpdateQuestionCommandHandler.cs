using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Questions.Commands.UpdateQuestion;

/// <summary>
/// Handler for UpdateQuestionCommand
/// </summary>
public class UpdateQuestionCommandHandler : IRequestHandler<UpdateQuestionCommand, Result<QuestionDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateQuestionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<QuestionDto>> Handle(
        UpdateQuestionCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Fetch existing question with options
        var question = await _context.Questions
            .Include(q => q.Options)
            .Include(q => q.SubTopic)
                .ThenInclude(st => st.Topic)
                    .ThenInclude(t => t.Subject)
                        .ThenInclude(s => s.StreamSubjects)
                            .ThenInclude(ss => ss.Stream)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (question is null)
            return Result<QuestionDto>.Failure("Question not found.");

        // 2. Validate options: require exactly five options and exactly one correct
        var optionList = request.Options ?? new List<UpdateOptionDto>();
        var correctCount = optionList.Count(o => o.IsCorrect);
        if (optionList.Count != 5)
            return Result<QuestionDto>.Failure("Exactly five options are required for this question type.");

        if (correctCount != 1)
            return Result<QuestionDto>.Failure("Exactly one correct answer is required.");

        // 3. Update scalar fields
        question.QuestionText     = request.QuestionText;
        question.QuestionImageUrl = request.QuestionImageUrl;
        question.Difficulty       = request.Difficulty;
        question.Marks            = request.Marks;
        question.OrderIndex       = request.OrderIndex;
        question.BloomLevel       = request.BloomLevel;
        question.ReviewStatus     = request.ReviewStatus;
        question.Tags             = request.Tags;
        question.IsFlagged        = request.IsFlagged;
        question.FlagReason       = request.FlagReason;

        // 4. Replace options — remove all old, add new
        _context.Options.RemoveRange(question.Options);

        var newOptions = request.Options.Select((o, index) => new Option
        {
            Id         = Guid.NewGuid(),
            QuestionId = question.Id,
            OptionText = o.OptionText,
            IsCorrect  = o.IsCorrect,
            OrderIndex = index
        }).ToList();

        await _context.Options.AddRangeAsync(newOptions, cancellationToken);

        // 5. Save
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Return updated DTO
        var dto = new QuestionDto
        {
            Id               = question.Id,
            PaperId          = question.PaperId,
            SubTopicId       = question.SubTopicId,
            SubTopicName     = question.SubTopic?.SubTopicName                       ?? string.Empty,
            TopicName        = question.SubTopic?.Topic?.TopicName                   ?? string.Empty,
            SubjectName      = question.SubTopic?.Topic?.Subject?.Name               ?? string.Empty,
            StreamName       = question.SubTopic?.Topic?.Subject?.StreamSubjects
                                    .Select(ss => ss.Stream.Name).FirstOrDefault() ?? string.Empty,
            QuestionText     = question.QuestionText,
            QuestionImageUrl = question.QuestionImageUrl,
            Difficulty       = question.Difficulty,
            Marks            = question.Marks,
            OrderIndex       = question.OrderIndex,
            BloomLevel       = question.BloomLevel,
            ReviewStatus     = question.ReviewStatus,
            Tags             = question.Tags
                                       .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(t => t.Trim())
                                       .ToList(),
            IsFlagged        = question.IsFlagged,
            FlagReason       = question.FlagReason,
            UsageCount       = question.UsageCount,
            SuccessRate      = question.SuccessRate,
            Options          = newOptions.Select(o => new OptionDto
            {
                Id         = o.Id,
                OptionText = o.OptionText,
                IsCorrect  = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).ToList()
        };

        return Result<QuestionDto>.Success(dto);
    }
}