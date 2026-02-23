using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Application.DTOs;

/// <summary>
/// DTO for QuestionReview (review materials attached to a question)
/// </summary>
public class QuestionReviewDto
{
    public Guid   Id           { get; set; }
    public Guid   QuestionId   { get; set; }
    public string Title        { get; set; } = string.Empty;
    public string Content      { get; set; } = string.Empty;
    public string? ResourceUrl { get; set; }
    public ReviewMaterialType MaterialType { get; set; }
    public int    DisplayOrder { get; set; }
    public bool   IsEssential  { get; set; }
}