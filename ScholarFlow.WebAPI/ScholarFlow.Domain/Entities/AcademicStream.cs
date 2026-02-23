using ScholarFlow.Domain.Entities.Base;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Represents an academic stream (e.g., Science, Commerce, Arts)
/// </summary>
public class AcademicStream : AuditableEntity
{
    private string _name = string.Empty;

    /// <summary>
    /// Name of the stream
    /// </summary>
    public string Name 
    { 
        get => _name;
        set => _name = value?.Trim() ?? string.Empty;
    }
    
    // Navigation properties - using backing fields to prevent lazy loading issues
    private readonly List<StudentProfile> _students = new();
    private readonly List<Subject> _subjects = new();

    /// <summary>
    /// Students enrolled in this stream
    /// </summary>
    public IReadOnlyCollection<StudentProfile> Students => _students.AsReadOnly();

    /// <summary>
    /// Subjects directly associated with this stream
    /// </summary>
    public IReadOnlyCollection<Subject> Subjects => _subjects.AsReadOnly();

    // Internal methods for EF Core
    internal void AddStudent(StudentProfile student) => _students.Add(student);
    internal void AddSubject(Subject subject) => _subjects.Add(subject);
}
