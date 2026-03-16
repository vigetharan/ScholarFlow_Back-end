using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.Application.Features.Papers.Queries.GetPapers;

public class GetPapersQuery : IRequest<Result<List<PaperDto>>>
{
    public Guid? StudentUserId { get; set; }
    public Guid? SubjectId { get; set; }
    public int? Year { get; set; }
    public PaperType? Type { get; set; }
    public string? TeacherCode { get; set; }
}
