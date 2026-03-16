using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Application.Features.Teachers.Commands.ReviewRegistration;

public class ReviewTeacherRegistrationCommand : IRequest<Result<TeacherApprovalRequestDto>>
{
    public Guid TeacherUserId { get; set; }
    public Guid ReviewedById { get; set; }
    public TeacherRegistrationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}
