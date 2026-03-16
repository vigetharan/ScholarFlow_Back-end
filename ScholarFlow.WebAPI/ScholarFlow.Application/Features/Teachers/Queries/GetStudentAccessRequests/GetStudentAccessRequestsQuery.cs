using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Application.Features.Teachers.Queries.GetStudentAccessRequests;

public class GetStudentAccessRequestsQuery : IRequest<Result<List<StudentAccessRequestDto>>>
{
    public Guid TeacherUserId { get; set; }
    public StudentTeacherConnectionStatus? Status { get; set; }
}