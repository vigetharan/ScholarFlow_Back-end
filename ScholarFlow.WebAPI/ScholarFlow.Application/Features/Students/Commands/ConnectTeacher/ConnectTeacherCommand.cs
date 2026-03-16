using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;

namespace ScholarFlow.Application.Features.Students.Commands.ConnectTeacher;

public class ConnectTeacherCommand : IRequest<Result<StudentTeacherConnectionDto>>
{
    public Guid StudentUserId { get; set; }
    public string TeacherCode { get; set; } = string.Empty;
}
