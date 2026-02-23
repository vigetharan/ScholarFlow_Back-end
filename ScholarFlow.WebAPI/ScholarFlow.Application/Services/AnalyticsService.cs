using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace ScholarFlow.Application.Services;

/// <summary>
/// Simple AI/ML analytics service without external AI dependencies
/// </summary>
public interface IAnalyticsService
{
    Task<decimal> CalculateQuestionDifficulty(Guid questionId);
    Task<decimal> CalculateStudentMastery(Guid studentId, Guid subjectId);
    Task<List<Guid>> RecommendQuestions(Guid studentId, Guid subjectId, int count);
    Task<List<string>> IdentifyWeakAreas(Guid studentId);
    Task<decimal> PredictExamPerformance(Guid studentId, Guid paperId);
}

public class SimpleAnalyticsService : IAnalyticsService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SimpleAnalyticsService> _logger;

    public SimpleAnalyticsService(IApplicationDbContext context, ILogger<SimpleAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Calculate question difficulty based on historical performance
    /// </summary>
    public async Task<decimal> CalculateQuestionDifficulty(Guid questionId)
    {
        var responses = await _context.UserResponses
            .Where(r => r.QuestionId == questionId)
            .ToListAsync();

        if (!responses.Any())
            return 50; // Default medium difficulty

        var correctCount = responses.Count(r => r.IsCorrect);
        var totalCount = responses.Count;
        var successRate = (decimal)correctCount / totalCount * 100;

        // Inverse success rate for difficulty (more correct = easier)
        var difficulty = 100 - successRate;

        // Adjust for time spent (slower answers might indicate harder questions)
        var avgTime = responses.Average(r => r.TimeSpentSeconds);
        if (avgTime > 60) // More than 1 minute average
            difficulty += 10;

        return Math.Clamp(difficulty, 0, 100);
    }

    /// <summary>
    /// Calculate student mastery level for a subject
    /// </summary>
    public async Task<decimal> CalculateStudentMastery(Guid studentId, Guid subjectId)
    {
        var recentSessions = await _context.ExamSessions
            .Include(es => es.UserResponses)
                .ThenInclude(ur => ur.Question)
                    .ThenInclude(q => q.Paper)
            .Where(es => es.UserId == studentId && 
                        es.Paper.SubjectId == subjectId &&
                        es.EndTime.HasValue)
            .OrderByDescending(es => es.StartTime)
            .Take(10) // Last 10 sessions
            .ToListAsync();

        if (!recentSessions.Any())
            return 0;

        var totalScore = recentSessions.Average(es => es.FinalScore);
        var consistency = CalculateConsistency(recentSessions.Select(es => es.FinalScore));
        var improvement = CalculateImprovementTrend(recentSessions);

        // Weighted mastery calculation
        var mastery = (totalScore * 0.6m) + (consistency * 0.2m) + (improvement * 0.2m);
        return Math.Clamp(mastery, 0, 100);
    }

    /// <summary>
    /// Recommend questions based on student performance
    /// </summary>
    public async Task<List<Guid>> RecommendQuestions(Guid studentId, Guid subjectId, int count)
    {
        // Get student's weak areas
        var weakAreas = await IdentifyWeakAreas(studentId);
        
        // Get questions in weak areas with appropriate difficulty
        var recommendedQuestions = await _context.Questions
            .Include(q => q.SubTopic)
            .Include(q => q.UserResponses)
            .Where(q => q.Paper.SubjectId == subjectId)
            .Where(q => weakAreas.Contains(q.SubTopic.SubTopicName))
            .OrderBy(q => Guid.NewGuid()) // Randomize
            .Take(count * 2) // Get more than needed
            .ToListAsync();

        // Filter by difficulty (not too hard, not too easy)
        var studentMastery = await CalculateStudentMastery(studentId, subjectId);
        var targetDifficulty = Math.Clamp(studentMastery + 10, 20, 80); // Slightly harder than current level

        var filtered = recommendedQuestions
            .Where(q => Math.Abs(q.CalculatedDifficulty - targetDifficulty) <= 20)
            .Take(count)
            .Select(q => q.Id)
            .ToList();

        return filtered;
    }

    /// <summary>
    /// Identify weak areas for a student
    /// </summary>
    public async Task<List<string>> IdentifyWeakAreas(Guid studentId)
    {
        var performance = await _context.StudentPerformanceAnalytics
            .Include(spa => spa.SubTopic)
            .Where(spa => spa.StudentId == studentId)
            .ToListAsync();

        var weakAreas = performance
            .Where(spa => spa.MasteryLevel < 60) // Below 60% mastery
            .OrderBy(spa => spa.MasteryLevel) // Weakest first
            .Select(spa => spa.SubTopic?.SubTopicName ?? "Unknown")
            .Take(5) // Top 5 weak areas
            .ToList();

        return weakAreas;
    }

    /// <summary>
    /// Predict exam performance based on historical data
    /// </summary>
    public async Task<decimal> PredictExamPerformance(Guid studentId, Guid paperId)
    {
        var paper = await _context.Papers
            .Include(p => p.Questions)
                .ThenInclude(q => q.SubTopic)
            .Include(p => p.Subject)
            .FirstAsync(p => p.Id == paperId);

        var studentSubjectMastery = await CalculateStudentMastery(studentId, paper.SubjectId);
        
        // Get performance in relevant subtopics
        var subTopicIds = paper.Questions.Select(q => q.SubTopicId).Distinct().ToList();
        var subTopicPerformance = await _context.StudentPerformanceAnalytics
            .Where(spa => spa.StudentId == studentId && subTopicIds.Contains(spa.SubTopicId!.Value))
            .ToListAsync();

        if (!subTopicPerformance.Any())
            return studentSubjectMastery; // Fall back to subject mastery

        var avgSubTopicMastery = subTopicPerformance.Average(spa => spa.MasteryLevel);
        var paperDifficulty = paper.Questions.Average(q => q.CalculatedDifficulty);

        // Simple prediction formula
        var predictedScore = (avgSubTopicMastery * 0.7m) + (studentSubjectMastery * 0.2m) - (paperDifficulty * 0.1m);
        
        return Math.Clamp(predictedScore, 0, 100);
    }

    private decimal CalculateConsistency(IEnumerable<decimal> scores)
    {
        var scoreList = scores.ToList();
        if (scoreList.Count < 2)
            return 100;

        var mean = scoreList.Average();
        var variance = scoreList.Average(s => Math.Pow((double)(s - mean), 2));
        var standardDeviation = Math.Sqrt(variance);

        // Lower standard deviation = higher consistency
        var consistency = Math.Max(0, 100 - (decimal)standardDeviation);
        return consistency;
    }

    private decimal CalculateImprovementTrend(List<ExamSession> sessions)
    {
        if (sessions.Count < 2)
            return 50; // Neutral

        var scores = sessions.Select(es => es.FinalScore).ToList();
        var firstHalf = scores.Take(scores.Count / 2).Average();
        var secondHalf = scores.Skip(scores.Count / 2).Average();

        var improvement = secondHalf - firstHalf;
        return Math.Clamp(50 + improvement, 0, 100); // Center around 50
    }
}
