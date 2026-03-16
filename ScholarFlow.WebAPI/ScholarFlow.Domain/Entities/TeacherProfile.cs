using ScholarFlow.Domain.Entities.Base;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Domain.Entities;

/// <summary>
/// Profile information for users with Teacher role
/// </summary>
public class TeacherProfile : BaseEntity
{
    private string _fullName = string.Empty;
    private string _qualification = string.Empty;
    private string _bio = string.Empty;
    private string _phoneNumber = string.Empty;

    /// <summary>
    /// Foreign key to ApplicationUser
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Subject specialization
    /// </summary>
    public Guid SubjectId { get; set; }

    /// <summary>
    /// Full name of the teacher
    /// </summary>
    public string FullName 
    { 
        get => _fullName;
        set => _fullName = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Academic qualifications
    /// </summary>
    public string Qualification 
    { 
        get => _qualification;
        set => _qualification = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Teacher biography
    /// </summary>
    public string Bio 
    { 
        get => _bio;
        set => _bio = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Teacher phone number
    /// </summary>
    public string PhoneNumber
    {
        get => _phoneNumber;
        set => _phoneNumber = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    /// Registration approval status
    /// </summary>
    public TeacherRegistrationStatus Status { get; set; } = TeacherRegistrationStatus.Pending;

    /// <summary>
    /// Permanent 6-character unique teacher code generated when accepted
    /// </summary>
    public string? TeacherCode { get; set; }

    /// <summary>
    /// Rejection reason set by admin
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Last review timestamp by admin
    /// </summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>
    /// Admin user id who last reviewed this profile
    /// </summary>
    public Guid? ReviewedById { get; set; }
    
    // Navigation property
    /// <summary>
    /// Associated user account
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Specialized subject
    /// </summary>
    public Subject Subject { get; set; } = null!;
}
