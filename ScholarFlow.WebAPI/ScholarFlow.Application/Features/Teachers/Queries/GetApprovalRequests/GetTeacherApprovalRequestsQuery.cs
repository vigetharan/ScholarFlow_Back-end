using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Application.Features.Teachers.Queries.GetApprovalRequests;

public class GetTeacherApprovalRequestsQuery : IRequest<Result<List<TeacherApprovalRequestDto>>>
{
    public TeacherRegistrationStatus? Status { get; set; }
}
