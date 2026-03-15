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
            .Include(q => q.Explanations)
                .ThenInclude(e => e.Sections)
            .Include(q => q.Paper)
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

        var normalizedExplanation = string.IsNullOrWhiteSpace(request.Explanation)
            ? null
            : request.Explanation.Trim();
        var normalizedExplanationTitle = normalizedExplanation is null
            ? null
            : (normalizedExplanation.Length <= 200
                ? normalizedExplanation
                : normalizedExplanation[..200]);
        var existingExplanation = question.Explanations.FirstOrDefault();
        if (normalizedExplanation is null)
        {
            if (existingExplanation is not null)
            {
                _context.Explanations.Remove(existingExplanation);
            }
        }
        else
        {
            if (existingExplanation is null)
            {
                var authorCandidateIds = new List<Guid>();
                if (question.Paper.CreatedByTeacher != Guid.Empty)
                    authorCandidateIds.Add(question.Paper.CreatedByTeacher);
                if (question.UpdatedBy.HasValue)
                    authorCandidateIds.Add(question.UpdatedBy.Value);
                if (question.CreatedBy.HasValue)
                    authorCandidateIds.Add(question.CreatedBy.Value);

                var explanationAuthorId = await _context.Users
                    .Where(u => authorCandidateIds.Contains(u.Id))
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (explanationAuthorId == Guid.Empty)
                {
                    return Result<QuestionDto>.Failure(
                        "Unable to add explanation because no valid author account is linked to this question.");
                }

                existingExplanation = new Explanation
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    AuthorId = explanationAuthorId,
                    Title = normalizedExplanationTitle
                };

                await _context.Explanations.AddAsync(existingExplanation, cancellationToken);
                await _context.ExplanationSections.AddAsync(new ExplanationSection
                {
                    Id = Guid.NewGuid(),
                    ExplanationId = existingExplanation.Id,
                    Type = Domain.Enums.ContentType.Text,
                    Content = normalizedExplanation,
                    OrderIndex = 0
                }, cancellationToken);
            }
            else
            {
                existingExplanation.Title = normalizedExplanationTitle;
                var textSection = existingExplanation.Sections
                    .Where(s => s.Type == Domain.Enums.ContentType.Text || s.Type == Domain.Enums.ContentType.Formula || s.Type == Domain.Enums.ContentType.Code)
                    .OrderBy(s => s.OrderIndex)
                    .FirstOrDefault();

                if (textSection is null)
                {
                    await _context.ExplanationSections.AddAsync(new ExplanationSection
                    {
                        Id = Guid.NewGuid(),
                        ExplanationId = existingExplanation.Id,
                        Type = Domain.Enums.ContentType.Text,
                        Content = normalizedExplanation,
                        OrderIndex = 0
                    }, cancellationToken);
                }
                else
                {
                    textSection.Content = normalizedExplanation;
                }
            }
        }

        // 4. Sync options in place to preserve Option IDs referenced by user responses.
        var existingOptions = question.Options.OrderBy(o => o.OrderIndex).ToList();
        var referencedOptionIds = await _context.UserResponses
            .Where(ur => ur.QuestionId == question.Id && ur.SelectedOptionId.HasValue)
            .Select(ur => ur.SelectedOptionId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var usedOptionIds = new HashSet<Guid>();
        var updatedOptions = new List<Option>(optionList.Count);

        for (var index = 0; index < optionList.Count; index++)
        {
            var incoming = optionList[index];
            Option? target = null;

            if (incoming.Id.HasValue)
            {
                target = existingOptions.FirstOrDefault(o => o.Id == incoming.Id.Value);
            }

            target ??= existingOptions.FirstOrDefault(o => o.OrderIndex == index && !usedOptionIds.Contains(o.Id));

            if (target is null)
            {
                target = new Option
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id
                };

                await _context.Options.AddAsync(target, cancellationToken);
            }

            target.OptionText = incoming.OptionText;
            target.IsCorrect = incoming.IsCorrect;
            target.OrderIndex = index;

            usedOptionIds.Add(target.Id);
            updatedOptions.Add(target);
        }

        var optionsToRemove = existingOptions.Where(o => !usedOptionIds.Contains(o.Id)).ToList();
        var blockedDeletes = optionsToRemove.Where(o => referencedOptionIds.Contains(o.Id)).ToList();
        if (blockedDeletes.Count > 0)
        {
            return Result<QuestionDto>.Failure(
                "Cannot remove options that are already used in student responses. Update existing options instead.");
        }

        if (optionsToRemove.Count > 0)
        {
            _context.Options.RemoveRange(optionsToRemove);
        }

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
            Explanation      = normalizedExplanation,
            Options          = updatedOptions.Select(o => new OptionDto
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