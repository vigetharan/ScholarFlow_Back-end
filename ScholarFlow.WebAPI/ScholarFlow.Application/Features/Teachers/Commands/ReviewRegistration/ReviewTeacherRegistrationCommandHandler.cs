using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Teachers.Commands.ReviewRegistration;

public class ReviewTeacherRegistrationCommandHandler : IRequestHandler<ReviewTeacherRegistrationCommand, Result<TeacherApprovalRequestDto>>
{
    private const string CodeChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private readonly IApplicationDbContext _context;

    public ReviewTeacherRegistrationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TeacherApprovalRequestDto>> Handle(ReviewTeacherRegistrationCommand request, CancellationToken cancellationToken)
    {
        var profile = await _context.TeacherProfiles
            .Include(t => t.User)
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.UserId == request.TeacherUserId, cancellationToken);

        if (profile == null)
        {
            return Result<TeacherApprovalRequestDto>.Failure("Teacher profile not found");
        }

        if (request.Status == TeacherRegistrationStatus.Accepted)
        {
            profile.Status = TeacherRegistrationStatus.Accepted;
            profile.RejectionReason = null;

            if (string.IsNullOrWhiteSpace(profile.TeacherCode))
            {
                profile.TeacherCode = await GenerateUniqueTeacherCode(cancellationToken);
            }
        }
        else
        {
            profile.Status = TeacherRegistrationStatus.Rejected;
            profile.RejectionReason = request.RejectionReason?.Trim();
        }

        profile.ReviewedAt = DateTime.UtcNow;
        profile.ReviewedById = request.ReviewedById;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<TeacherApprovalRequestDto>.Success(new TeacherApprovalRequestDto
        {
            UserId = profile.UserId,
            ProfileId = profile.Id,
            Email = profile.User.Email ?? string.Empty,
            FullName = profile.FullName,
            Qualification = profile.Qualification,
            SubjectId = profile.SubjectId,
            SubjectName = profile.Subject.Name,
            PhoneNumber = profile.PhoneNumber,
            Status = profile.Status.ToString(),
            TeacherCode = profile.TeacherCode,
            RejectionReason = profile.RejectionReason,
            ReviewedAt = profile.ReviewedAt
        });
    }

    private async Task<string> GenerateUniqueTeacherCode(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var code = GenerateCode();
            var exists = await _context.TeacherProfiles.AnyAsync(t => t.TeacherCode == code, cancellationToken);
            if (!exists)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Failed to generate a unique teacher code.");
    }

    private static string GenerateCode()
    {
        var sb = new StringBuilder(6);
        for (var i = 0; i < 6; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(CodeChars.Length);
            sb.Append(CodeChars[idx]);
        }

        return sb.ToString();
    }
}
