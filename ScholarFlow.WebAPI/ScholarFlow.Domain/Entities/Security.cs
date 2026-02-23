using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Exam session security and proctoring
/// </summary>
public class ExamSessionSecurity : AuditableEntity
{
    /// <summary>
    /// Foreign key to ExamSession
    /// </summary>
    public Guid ExamSessionId { get; set; }

    // Proctoring Settings
    /// <summary>
    /// Enable webcam monitoring
    /// </summary>
    public bool WebcamMonitoring { get; set; }

    /// <summary>
    /// Enable screen recording
    /// </summary>
    public bool ScreenRecording { get; set; }

    /// <summary>
    /// Enable tab switching detection
    /// </summary>
    public bool TabSwitchingDetection { get; set; }

    /// <summary>
    /// Enable copy/paste detection
    /// </summary>
    public bool CopyPasteDetection { get; set; }

    /// <summary>
    /// Enable right-click detection
    /// </summary>
    public bool RightClickDetection { get; set; }

    /// <summary>
    /// Enable keyboard shortcuts detection
    /// </summary>
    public bool KeyboardShortcutsDetection { get; set; }

    // IP & Location Restrictions
    /// <summary>
    /// Restrict by IP address
    /// </summary>
    public bool IPRestriction { get; set; }

    /// <summary>
    /// Allowed IP addresses (JSON array)
    /// </summary>
    public string? AllowedIPs { get; set; }

    /// <summary>
    /// Restrict by geographic location
    /// </summary>
    public bool LocationRestriction { get; set; }

    /// <summary>
    /// Allowed countries (JSON array)
    /// </summary>
    public string? AllowedCountries { get; set; }

    // Time & Access Controls
    /// <summary>
    /// Server time validation
    /// </summary>
    public bool ServerTimeValidation { get; set; }

    /// <summary>
    /// Allowed time deviation (seconds)
    /// </summary>
    public int AllowedTimeDeviation { get; set; } = 300;

    /// <summary>
    /// Single session per user
    /// </summary>
    public bool SingleSessionPerUser { get; set; }

    /// <summary>
    /// Maximum concurrent sessions
    /// </summary>
    public int MaxConcurrentSessions { get; set; } = 1;

    // Navigation properties
    /// <summary>
    /// Exam session being secured
    /// </summary>
    public ExamSession ExamSession { get; set; } = null!;
}

/// <summary>
/// Security events and violations
/// </summary>
public class SecurityEvent : AuditableEntity
{
    /// <summary>
    /// Foreign key to ExamSession
    /// </summary>
    public Guid ExamSessionId { get; set; }

    /// <summary>
    /// Foreign key to User
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of security event
    /// </summary>
    public SecurityEventType EventType { get; set; }

    /// <summary>
    /// Event description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event severity
    /// </summary>
    public SecuritySeverity Severity { get; set; }

    /// <summary>
    /// IP address of the event
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// User agent string
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Event data (JSON)
    /// </summary>
    public string? EventData { get; set; }

    /// <summary>
    /// Whether this event was auto-resolved
    /// </summary>
    public bool AutoResolved { get; set; }

    /// <summary>
    /// Resolution notes
    /// </summary>
    public string? ResolutionNotes { get; set; }

    // Navigation properties
    /// <summary>
    /// Exam session where event occurred
    /// </summary>
    public ExamSession ExamSession { get; set; } = null!;

    /// <summary>
    /// User who triggered the event
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}

/// <summary>
/// Browser lockdown settings
/// </summary>
public class BrowserLockdown : AuditableEntity
{
    /// <summary>
    /// Foreign key to ExamSession
    /// </summary>
    public Guid ExamSessionId { get; set; }

    // Lockdown Features
    /// <summary>
    /// Disable right-click
    /// </summary>
    public bool DisableRightClick { get; set; }

    /// <summary>
    /// Disable keyboard shortcuts
    /// </summary>
    public bool DisableKeyboardShortcuts { get; set; }

    /// <summary>
    /// Disable copy/paste
    /// </summary>
    public bool DisableCopyPaste { get; set; }

    /// <summary>
    /// Disable print functionality
    /// </summary>
    public bool DisablePrint { get; set; }

    /// <summary>
    /// Disable developer tools
    /// </summary>
    public bool DisableDeveloperTools { get; set; }

    /// <summary>
    /// Full screen mode required
    /// </summary>
    public bool RequireFullScreen { get; set; }

    /// <summary>
    /// Disable tab switching
    /// </summary>
    public bool DisableTabSwitching { get; set; }

    /// <summary>
    /// Disable browser navigation
    /// </summary>
    public bool DisableNavigation { get; set; }

    // Navigation properties
    /// <summary>
    /// Exam session with lockdown
    /// </summary>
    public ExamSession ExamSession { get; set; } = null!;
}

public enum SecurityEventType
{
    TabSwitch = 1,
    CopyAttempt = 2,
    PasteAttempt = 3,
    RightClick = 4,
    KeyboardShortcut = 5,
    WindowResize = 6,
    FullscreenExit = 7,
    MultipleSessions = 8,
    IPViolation = 9,
    TimeViolation = 10,
    SuspiciousActivity = 11,
    PlagiarismDetected = 12
}

public enum SecuritySeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
