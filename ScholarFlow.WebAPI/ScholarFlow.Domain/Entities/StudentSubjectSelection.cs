using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Junction entity for student-selected core subjects (max 3 per student profile)
/// </summary>
public class StudentSubjectSelection : BaseEntity
{
    /// <summary>
    /// Foreign key to StudentProfile
    /// </summary>
    public Guid StudentProfileId { get; set; }

    /// <summary>
    /// Foreign key to Subject
    /// </summary>
    public Guid SubjectId { get; set; }

    /// <summary>
    /// Student profile that selected the subject
    /// </summary>
    public StudentProfile StudentProfile { get; set; } = null!;

    /// <summary>
    /// Selected subject
    /// </summary>
    public Subject Subject { get; set; } = null!;
}
