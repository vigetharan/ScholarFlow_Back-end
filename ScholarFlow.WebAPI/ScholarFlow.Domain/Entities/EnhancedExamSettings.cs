using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Enhanced settings for exam papers
/// </summary>
public class EnhancedExamSettings : AuditableEntity
{
    /// <summary>
    /// Foreign key to Paper
    /// </summary>
    public Guid PaperId { get; set; }

    // Navigation & Display Settings
    /// <summary>
    /// Randomize question order
    /// </summary>
    public bool RandomizeQuestions { get; set; } = false;

    /// <summary>
    /// Randomize answer options
    /// </summary>
    public bool RandomizeOptions { get; set; } = false;

    /// <summary>
    /// Allow question navigation (back/forward)
    /// </summary>
    public bool AllowQuestionNavigation { get; set; } = true;

    /// <summary>
    /// Show questions in grid view
    /// </summary>
    public bool ShowQuestionGrid { get; set; } = true;

    // Timing Settings
    /// <summary>
    /// Auto-submit when time expires
    /// </summary>
    public bool AutoSubmitOnTimeout { get; set; } = true;

    /// <summary>
    /// Show timer to user
    /// </summary>
    public bool ShowTimer { get; set; } = true;

    /// <summary>
    /// Show time per question
    /// </summary>
    public bool ShowPerQuestionTimer { get; set; } = false;

    // Results & Feedback
    /// <summary>
    /// Show results immediately after submission
    /// </summary>
    public bool ShowResultsImmediately { get; set; } = true;

    /// <summary>
    /// Show correct answers after submission
    /// </summary>
    public bool ShowCorrectAnswers { get; set; } = true;

    /// <summary>
    /// Show explanations for answers
    /// </summary>
    public bool ShowExplanations { get; set; } = true;

    /// <summary>
    /// Allow review of attempted questions
    /// </summary>
    public bool AllowReview { get; set; } = true;

    // Attempt & Scoring
    /// <summary>
    /// Maximum number of attempts allowed
    /// </summary>
    public int MaxAttempts { get; set; } = 1;

    /// <summary>
    /// Passing score percentage
    /// </summary>
    public decimal PassingScore { get; set; } = 60;

    /// <summary>
    /// Negative marking for wrong answers
    /// </summary>
    public decimal NegativeMarking { get; set; } = 0;

    /// <summary>
    /// Marks for unattempted questions
    /// </summary>
    public decimal UnattemptedMarks { get; set; } = 0;

    // Review Materials
    /// <summary>
    /// Show review materials for wrong answers
    /// </summary>
    public bool ShowReviewMaterials { get; set; } = true;

    /// <summary>
    /// Show review materials only after exam completion
    /// </summary>
    public bool ShowReviewAfterCompletion { get; set; } = true;

    // Navigation properties
    /// <summary>
    /// Paper these settings belong to
    /// </summary>
    public Paper Paper { get; set; } = null!;
}
