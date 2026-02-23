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
                        .ThenInclude(s => s.Stream)
            .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

        if (question is null)
            return Result<QuestionDto>.Failure("Question not found.");

        // 2. Validate options
        var correctCount = request.Options.Count(o => o.IsCorrect);
        if (correctCount != 1)
            return Result<QuestionDto>.Failure("Exactly one correct answer is required.");

        if (request.Options.Count < 4 || request.Options.Count > 5)
            return Result<QuestionDto>.Failure("Questions must have between 4 and 5 options.");

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
            OrderIndex = index + 1
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
            StreamName       = question.SubTopic?.Topic?.Subject?.Stream?.Name       ?? string.Empty,
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