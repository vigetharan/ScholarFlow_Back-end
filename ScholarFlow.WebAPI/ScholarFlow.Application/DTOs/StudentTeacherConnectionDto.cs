namespace ScholarFlow.Application.DTOs;

public class StudentTeacherConnectionDto
{
    public Guid TeacherUserId { get; set; }
    public string TeacherCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ReviewedAt { get; set; }
}
