using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;

namespace ScholarFlow.Application.Features.Questions.Queries.GetAllQuestions;

public class GetAllQuestionsQuery : IRequest<Result<List<QuestionDto>>>
{
	public Guid? PaperId { get; set; }
}