using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Question bank for organizing and managing questions
/// </summary>
public class QuestionBank : AuditableEntity
{
    /// <summary>
    /// Name of the question bank
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the question bank
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this bank is public or private
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Owner of the question bank
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Category/Subject of this bank
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Difficulty level range (min-max)
    /// </summary>
    public string DifficultyRange { get; set; } = string.Empty;

    /// <summary>
    /// Total questions in bank
    /// </summary>
    public int TotalQuestions { get; set; }

    /// <summary>
    /// Average quality score
    /// </summary>
    public decimal AverageQualityScore { get; set; }

    /// <summary>
    /// Tags for searchability
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Download count
    /// </summary>
    public int DownloadCount { get; set; }

    // Navigation properties
    /// <summary>
    /// Owner of the question bank
    /// </summary>
    public ApplicationUser Owner { get; set; } = null!;

    /// <summary>
    /// Questions in this bank
    /// </summary>
    public IReadOnlyCollection<Question> Questions { get; set; } = new List<Question>().AsReadOnly();
}

/// <summary>
/// Question import/export operations
/// </summary>
public class QuestionImport : AuditableEntity
{
    /// <summary>
    /// Import operation name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Source type (CSV, JSON, Word, PDF, etc.)
    /// </summary>
    public ImportSourceType SourceType { get; set; }

    /// <summary>
    /// Current status of import
    /// </summary>
    public ImportStatus Status { get; set; }

    /// <summary>
    /// Total records to process
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Successfully processed records
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// Failed records count
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Duplicate records count
    /// </summary>
    public int DuplicateCount { get; set; }

    /// <summary>
    /// Error details (JSON)
    /// </summary>
    public string? Errors { get; set; }

    /// <summary>
    /// File path of imported file
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Target question bank (optional)
    /// </summary>
    public Guid? TargetQuestionBankId { get; set; }

    /// <summary>
    /// Import settings (JSON)
    /// </summary>
    public string? ImportSettings { get; set; }

    /// <summary>
    /// Started at
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Completed at
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// User who initiated the import
    /// </summary>
    public ApplicationUser ImportedBy { get; set; } = null!;

    /// <summary>
    /// Target question bank
    /// </summary>
    public QuestionBank? TargetQuestionBank { get; set; }
}

/// <summary>
/// Content versioning for questions
/// </summary>
public class QuestionVersion : AuditableEntity
{
    /// <summary>
    /// Foreign key to Question
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Version number
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// Question text at this version
    /// </summary>
    public string QuestionText { get; set; } = string.Empty;

    /// <summary>
    /// Options at this version (JSON)
    /// </summary>
    public string OptionsData { get; set; } = string.Empty;

    /// <summary>
    /// Change summary
    /// </summary>
    public string ChangeSummary { get; set; } = string.Empty;

    /// <summary>
    /// Reason for change
    /// </summary>
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>
    /// Whether this version is active
    /// </summary>
    public bool IsActive { get; set; }

    // Navigation properties
    /// <summary>
    /// Question this version belongs to
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// User who made this change
    /// </summary>
    public ApplicationUser ChangedBy { get; set; } = null!;
}

public enum ImportSourceType
{
    CSV = 1,
    JSON = 2,
    Word = 3,
    PDF = 4,
    Excel = 5,
    XML = 6,
    API = 7
}

public enum ImportStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}
