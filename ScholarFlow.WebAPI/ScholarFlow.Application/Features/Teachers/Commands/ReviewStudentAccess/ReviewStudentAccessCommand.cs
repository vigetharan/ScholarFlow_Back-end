using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Application.Features.Teachers.Commands.ReviewStudentAccess;

public class ReviewStudentAccessCommand : IRequest<Result<StudentTeacherConnectionDto>>
{
    public Guid TeacherUserId { get; set; }
    public Guid StudentUserId { get; set; }
    public StudentTeacherConnectionStatus Status { get; set; }
}