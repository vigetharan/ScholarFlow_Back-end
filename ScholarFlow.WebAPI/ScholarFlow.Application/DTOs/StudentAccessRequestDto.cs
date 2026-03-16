namespace ScholarFlow.Application.DTOs;

public class StudentAccessRequestDto
{
    public Guid StudentUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string TeacherCode { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ReviewedAt { get; set; }
}