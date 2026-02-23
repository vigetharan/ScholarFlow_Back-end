using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Represents a question in an exam paper
/// </summary>
public partial class Question : AuditableEntity
{
    private string _questionText = string.Empty;
    private string? _questionImageUrl;

    /// <summary>
    /// Foreign key to Paper
    /// </summary>
    public Guid PaperId { get; set; }

    /// <summary>
    /// Foreign key to SubTopic
    /// </summary>
    public Guid SubTopicId { get; set; }

    /// <summary>
    /// Question text (supports LaTeX/HTML)
    /// </summary>
    public string QuestionText 
    { 
        get => _questionText;
        set => _questionText = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Optional image URL for the question
    /// </summary>
    public string? QuestionImageUrl 
    { 
        get => _questionImageUrl;
        set => _questionImageUrl = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Difficulty level (1-10)
    /// </summary>
    public int Difficulty { get; set; }

    /// <summary>
    /// Marks for this question
    /// </summary>
    public decimal Marks { get; set; } = 1;

    /// <summary>
    /// Order/sequence of question in the paper
    /// </summary>
    public int OrderIndex { get; set; }
    
    // Navigation properties
    private readonly List<Option> _options = new();
    private readonly List<Explanation> _explanations = new();
    private readonly List<UserResponse> _userResponses = new();

    /// <summary>
    /// Paper this question belongs to
    /// </summary>
    public Paper Paper { get; set; } = null!;

    /// <summary>
    /// Sub-topic this question is categorized under
    /// </summary>
    public SubTopic SubTopic { get; set; } = null!;

    /// <summary>
    /// Answer options for this question
    /// </summary>
    public IReadOnlyCollection<Option> Options => _options.AsReadOnly();

    /// <summary>
    /// Explanations for this question
    /// </summary>
    public IReadOnlyCollection<Explanation> Explanations => _explanations.AsReadOnly();

    /// <summary>
    /// User responses to this question
    /// </summary>
    public IReadOnlyCollection<UserResponse> UserResponses => _userResponses.AsReadOnly();

    // Enhanced Analytics Properties
    /// <summary>
    /// AI-calculated difficulty (0-100)
    /// </summary>
    public decimal CalculatedDifficulty { get; set; }

    /// <summary>
    /// Bloom's taxonomy level
    /// </summary>
    public BloomTaxonomyLevel BloomLevel { get; set; }

    /// <summary>
    /// Question tags for categorization
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Number of times this question was used
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Historical success rate (0-100)
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Average time to answer (seconds)
    /// </summary>
    public decimal AverageTimeToAnswer { get; set; }

    /// <summary>
    /// Last updated analytics
    /// </summary>
    public DateTime AnalyticsLastUpdated { get; set; }

    /// <summary>
    /// Review status for quality control
    /// </summary>
    public QuestionReviewStatus ReviewStatus { get; set; }

    /// <summary>
    /// Contributor who created/modified this question
    /// </summary>
    public Guid ContributorId { get; set; }

    /// <summary>
    /// Question quality score (0-100)
    /// </summary>
    public decimal QualityScore { get; set; }

    /// <summary>
    /// Whether this question is flagged for issues
    /// </summary>
    public bool IsFlagged { get; set; }

    /// <summary>
    /// Flag reason
    /// </summary>
    public string? FlagReason { get; set; }

    /// <summary>
    /// Analytics for this question
    /// </summary>
    public IReadOnlyCollection<QuestionAttemptAnalytics> AttemptAnalytics { get; set; } = new List<QuestionAttemptAnalytics>().AsReadOnly();

    internal void AddOption(Option option) => _options.Add(option);
    internal void AddExplanation(Explanation explanation) => _explanations.Add(explanation);
    internal void AddUserResponse(UserResponse response) => _userResponses.Add(response);
}

public enum BloomTaxonomyLevel
{
    Remember = 1,
    Understand = 2,
    Apply = 3,
    Analyze = 4,
    Evaluate = 5,
    Create = 6
}

public enum QuestionReviewStatus
{
    Pending = 1,
    Approved = 2,
    NeedsRevision = 3,
    Rejected = 4
}
