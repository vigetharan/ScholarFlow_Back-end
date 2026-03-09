using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Junction entity for many-to-many relationship between AcademicStream and Subject
/// </summary>
public class StreamSubject : AuditableEntity
{
    /// <summary>
    /// Foreign key to AcademicStream
    /// </summary>
    public Guid StreamId { get; set; }

    /// <summary>
    /// Foreign key to Subject
    /// </summary>
    public Guid SubjectId { get; set; }

    // Navigation properties
    /// <summary>
    /// Academic stream this subject belongs to
    /// </summary>
    public AcademicStream Stream { get; set; } = null!;

    /// <summary>
    /// Subject that belongs to this stream
    /// </summary>
    public Subject Subject { get; set; } = null!;
}
