namespace ScholarFlow.Application.DTOs;

public class TeacherApprovalRequestDto
{
    public Guid UserId { get; set; }
    public Guid ProfileId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TeacherCode { get; set; }
    public int PaperCount { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
