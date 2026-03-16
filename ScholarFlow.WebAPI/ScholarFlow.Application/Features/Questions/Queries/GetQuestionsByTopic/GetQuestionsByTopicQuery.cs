using MediatR;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;

namespace ScholarFlow.Application.Features.Questions.Queries.GetQuestionsByTopic;

/// <summary>
/// Query to get questions by topic.
/// </summary>
public class GetQuestionsByTopicQuery : IRequest<Result<List<QuestionDto>>>
{
    public Guid TopicId { get; set; }
    public int? Limit { get; set; }
}
