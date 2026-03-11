namespace ScholarFlow.Application.DTOs;

/// <summary>
/// Subject data transfer object
/// </summary>
public class SubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> StreamNames { get; set; } = new();
    public List<Guid> StreamIds { get; set; } = new();
    public int TopicCount { get; set; } = 0;
}
