using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Application.DTOs;

/// <summary>
/// Question data transfer object
/// </summary>
public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid PaperId { get; set; }
    public Guid SubTopicId { get; set; }

    // Resolved display names (from navigation properties)
    public string SubTopicName { get; set; } = string.Empty;
    public string TopicName    { get; set; } = string.Empty;
    public string SubjectName  { get; set; } = string.Empty;
    public string StreamName   { get; set; } = string.Empty;

    public string  QuestionText     { get; set; } = string.Empty;
    public string? QuestionImageUrl { get; set; }
    public int     Difficulty       { get; set; }
    public decimal Marks            { get; set; }
    public int     OrderIndex       { get; set; }

    public BloomTaxonomyLevel   BloomLevel   { get; set; }
    public QuestionReviewStatus ReviewStatus { get; set; }
    public List<string>         Tags         { get; set; } = new();
    public bool                 IsFlagged    { get; set; }
    public string?              FlagReason   { get; set; }
    public int                  UsageCount   { get; set; }
    public decimal              SuccessRate  { get; set; }
    public string?              Explanation  { get; set; }

    public List<OptionDto>        Options         { get; set; } = new();
    public List<QuestionReviewDto> ReviewMaterials { get; set; } = new();
}