namespace ScholarFlow.Application.DTOs;

/// <summary>
/// Subject data transfer object
/// </summary>
public class SubjectDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid StreamId { get; set; }
    public string StreamName { get; set; } = string.Empty;
}
