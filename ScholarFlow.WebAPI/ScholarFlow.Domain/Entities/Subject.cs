using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Represents an academic subject (e.g., Mathematics, Physics, Chemistry)
/// </summary>
public class Subject : AuditableEntity
{
    private string _name = string.Empty;

    /// <summary>
    /// Name of the subject
    /// </summary>
    public string Name 
    { 
        get => _name;
        set => _name = value?.Trim() ?? string.Empty;
    }
    
    /// <summary>
    /// Direct reference to the academic stream this subject belongs to
    /// </summary>
    public Guid StreamId { get; set; }
    
    // Navigation properties with backing fields
    private AcademicStream? _stream;
    private readonly List<Topic> _topics = new();
    private readonly List<Paper> _papers = new();

    /// <summary>
    /// Academic stream this subject belongs to
    /// </summary>
    public AcademicStream? Stream { get; set; }

    /// <summary>
    /// Topics under this subject
    /// </summary>
    public IReadOnlyCollection<Topic> Topics => _topics.AsReadOnly();

    /// <summary>
    /// Papers for this subject
    /// </summary>
    public IReadOnlyCollection<Paper> Papers => _papers.AsReadOnly();

    // Internal methods for EF Core
    internal void AddTopic(Topic topic) => _topics.Add(topic);
    internal void AddPaper(Paper paper) => _papers.Add(paper);
}
