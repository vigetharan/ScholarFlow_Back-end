using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Teachers.Commands.CreateProfile;

/// <summary>
/// Handler for CreateTeacherProfileCommand
/// </summary>
public class CreateTeacherProfileCommandHandler : IRequestHandler<CreateTeacherProfileCommand, Result<TeacherProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateTeacherProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TeacherProfileDto>> Handle(CreateTeacherProfileCommand request, CancellationToken cancellationToken)
    {
        // Check if profile already exists for this user
        var existingProfile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (existingProfile != null)
        {
            return Result<TeacherProfileDto>.Failure("Teacher profile already exists");
        }

        var subjectExists = await _context.Subjects
            .AnyAsync(s => s.Id == request.SubjectId, cancellationToken);

        if (!subjectExists)
        {
            return Result<TeacherProfileDto>.Failure("Subject not found");
        }

        // Create teacher profile
        var profile = new TeacherProfile
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FullName = request.FullName,
            Qualification = request.Qualification,
            Bio = request.Bio ?? string.Empty,
            SubjectId = request.SubjectId,
            PhoneNumber = request.PhoneNumber
        };

        _context.TeacherProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var dto = new TeacherProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = profile.FullName,
            Qualification = profile.Qualification,
            Bio = profile.Bio,
            SubjectId = profile.SubjectId,
            PhoneNumber = profile.PhoneNumber,
            Status = profile.Status.ToString(),
            TeacherCode = profile.TeacherCode,
            RejectionReason = profile.RejectionReason,
            ReviewedAt = profile.ReviewedAt
        };

        return Result<TeacherProfileDto>.Success(dto);
    }
}
