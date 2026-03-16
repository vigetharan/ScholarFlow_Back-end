using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Teachers.Commands.UpdateProfile;

/// <summary>
/// Handler for UpdateTeacherProfileCommand
/// </summary>
public class UpdateTeacherProfileCommandHandler : IRequestHandler<UpdateTeacherProfileCommand, Result<TeacherProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateTeacherProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TeacherProfileDto>> Handle(UpdateTeacherProfileCommand request, CancellationToken cancellationToken)
    {
        // Get profile for this user
        var profile = await _context.TeacherProfiles
            .FirstOrDefaultAsync(t => t.UserId == request.UserId, cancellationToken);

        if (profile == null)
        {
            return Result<TeacherProfileDto>.Failure("Teacher profile not found");
        }

        var subjectExists = await _context.Subjects
            .AnyAsync(s => s.Id == request.SubjectId, cancellationToken);

        if (!subjectExists)
        {
            return Result<TeacherProfileDto>.Failure("Subject not found");
        }

        // Update profile
        profile.FullName = request.FullName;
        profile.Qualification = request.Qualification;
        profile.Bio = request.Bio ?? string.Empty;
        profile.SubjectId = request.SubjectId;
        profile.PhoneNumber = request.PhoneNumber;

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
