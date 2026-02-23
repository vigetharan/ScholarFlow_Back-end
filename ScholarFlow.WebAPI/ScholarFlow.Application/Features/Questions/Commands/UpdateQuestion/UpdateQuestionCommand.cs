using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Application.Features.Questions.Commands.UpdateQuestion;

public class UpdateQuestionCommand : IRequest<Result<QuestionDto>>
{
    public Guid Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public int Difficulty { get; set; }
    public decimal Marks { get; set; }
    public int OrderIndex { get; set; }
    public BloomTaxonomyLevel BloomLevel { get; set; }
    public QuestionReviewStatus ReviewStatus { get; set; }
    public string Tags { get; set; } = string.Empty;
    public bool IsFlagged { get; set; }
    public string? FlagReason { get; set; }
    public List<UpdateOptionDto> Options { get; set; } = new();
    public string? Explanation { get; set; }
}

public class UpdateOptionDto
{
    public Guid? Id { get; set; }          // null = new option
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}