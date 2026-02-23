using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Detailed analytics for student performance
/// </summary>
public class StudentPerformanceAnalytics : AuditableEntity
{
    /// <summary>
    /// Foreign key to User (Student)
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Foreign key to Subject
    /// </summary>
    public Guid SubjectId { get; set; }

    /// <summary>
    /// Foreign key to Topic (optional)
    /// </summary>
    public Guid? TopicId { get; set; }

    /// <summary>
    /// Foreign key to SubTopic (optional)
    /// </summary>
    public Guid? SubTopicId { get; set; }

    // Performance Metrics
    /// <summary>
    /// Overall mastery level (0-100)
    /// </summary>
    public decimal MasteryLevel { get; set; }

    /// <summary>
    /// Average score percentage
    /// </summary>
    public decimal AverageScore { get; set; }

    /// <summary>
    /// Total questions attempted
    /// </summary>
    public int TotalQuestionsAttempted { get; set; }

    /// <summary>
    /// Correct answers count
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// Accuracy rate (0-100)
    /// </summary>
    public decimal AccuracyRate { get; set; }

    /// <summary>
    /// Average time per question (seconds)
    /// </summary>
    public decimal AverageTimePerQuestion { get; set; }

    /// <summary>
    /// Strength indicator (0-100)
    /// </summary>
    public decimal StrengthIndicator { get; set; }

    /// <summary>
    /// Weakness indicator (0-100)
    /// </summary>
    public decimal WeaknessIndicator { get; set; }

    // Behavioral Analytics
    /// <summary>
    /// Preferred question difficulty
    /// </summary>
    public int PreferredDifficulty { get; set; }

    /// <summary>
    /// Common mistake patterns (JSON)
    /// </summary>
    public string? MistakePatterns { get; set; }

    /// <summary>
    /// Learning pace (questions per hour)
    /// </summary>
    public decimal LearningPace { get; set; }

    /// <summary>
    /// Consistency score (0-100)
    /// </summary>
    public decimal ConsistencyScore { get; set; }

    /// <summary>
    /// Last activity date
    /// </summary>
    public DateTime LastActivityDate { get; set; }

    // Recommendations
    /// <summary>
    /// Recommended study topics (JSON array)
    /// </summary>
    public string? RecommendedTopics { get; set; }

    /// <summary>
    /// Recommended difficulty level
    /// </summary>
    public int RecommendedDifficulty { get; set; }

    /// <summary>
    /// Study priority (1-10)
    /// </summary>
    public int StudyPriority { get; set; }

    // Navigation properties
    /// <summary>
    /// Student being analyzed
    /// </summary>
    public ApplicationUser Student { get; set; } = null!;

    /// <summary>
    /// Subject of analysis
    /// </summary>
    public Subject Subject { get; set; } = null!;

    /// <summary>
    /// Topic of analysis (optional)
    /// </summary>
    public Topic? Topic { get; set; }

    /// <summary>
    /// SubTopic of analysis (optional)
    /// </summary>
    public SubTopic? SubTopic { get; set; }
}

/// <summary>
/// Detailed question attempt analytics
/// </summary>
public class QuestionAttemptAnalytics : AuditableEntity
{
    /// <summary>
    /// Foreign key to User Response
    /// </summary>
    public Guid UserResponseId { get; set; }

    /// <summary>
    /// Time to first answer (seconds)
    /// </summary>
    public int TimeToFirstAnswer { get; set; }

    /// <summary>
    /// Number of times answer was changed
    /// </summary>
    public int AnswerChanges { get; set; }

    /// <summary>
    /// Time spent reviewing question
    /// </summary>
    public int ReviewTimeSpent { get; set; }

    /// <summary>
    /// Confidence level (1-5)
    /// </summary>
    public int ConfidenceLevel { get; set; }

    /// <summary>
    /// Whether question was marked for review
    /// </summary>
    public bool MarkedForReview { get; set; }

    /// <summary>
    /// Question difficulty vs user difficulty match
    /// </summary>
    public decimal DifficultyMatch { get; set; }

    /// <summary>
    /// Performance category (Excellent/Good/Average/Poor)
    /// </summary>
    public string PerformanceCategory { get; set; } = string.Empty;

    // Navigation properties
    /// <summary>
    /// User response being analyzed
    /// </summary>
    public UserResponse UserResponse { get; set; } = null!;
}
