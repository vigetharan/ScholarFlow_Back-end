using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Review materials for questions answered incorrectly
/// </summary>
public class QuestionReview : AuditableEntity
{
    /// <summary>
    /// Foreign key to Question
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Type of review material
    /// </summary>
    public ReviewMaterialType MaterialType { get; set; }

    /// <summary>
    /// Title of the review material
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Content/Description
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// URL for external resources (video, article, etc.)
    /// </summary>
    public string? ResourceUrl { get; set; }

    /// <summary>
    /// Order of display
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this material is essential for understanding
    /// </summary>
    public bool IsEssential { get; set; }

    // Navigation properties
    /// <summary>
    /// Question this review belongs to
    /// </summary>
    public Question Question { get; set; } = null!;
}

public enum ReviewMaterialType
{
    Text = 1,
    Image = 2,
    Video = 3,
    Article = 4,
    Infographic = 5,
    Animation = 6
}
