namespace ScholarFlow.Application.DTOs;

/// <summary>
/// AcademicStream Data Transfer Object
/// </summary>
public class StreamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> SubjectNames { get; set; } = new();
    public List<Guid> SubjectIds { get; set; } = new();
}
