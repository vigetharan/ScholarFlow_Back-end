using MediatR;
using ScholarFlow.Application.Common.Models;

namespace ScholarFlow.Application.Features.Students.Commands.DisconnectTeacher;

public class DisconnectTeacherCommand : IRequest<Result<bool>>
{
    public Guid StudentUserId { get; set; }
    public Guid TeacherUserId { get; set; }
}
