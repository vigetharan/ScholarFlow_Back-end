using ScholarFlow.Domain.Entities.Base;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Persistent mapping between a student and a connected teacher.
/// </summary>
public class StudentTeacherConnection : BaseEntity
{
    public Guid StudentUserId { get; set; }
    public Guid TeacherUserId { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public StudentTeacherConnectionStatus Status { get; set; } = StudentTeacherConnectionStatus.Pending;
    public DateTime? ReviewedAt { get; set; }

    public ApplicationUser StudentUser { get; set; } = null!;
    public ApplicationUser TeacherUser { get; set; } = null!;
}
