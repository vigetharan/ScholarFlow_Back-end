using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;

namespace ScholarFlow.Application.Features.Students.Queries.GetConnectedTeachers;

public class GetConnectedTeachersQuery : IRequest<Result<List<StudentTeacherConnectionDto>>>
{
    public Guid StudentUserId { get; set; }
}
