// Temporarily commented out due to SignalR compatibility issues with .NET 10.0
// TODO: Re-enable when SignalR is compatible or use alternative real-time solution

/*
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Application.Features.RealTime;

/// <summary>
/// SignalR Hub for real-time exam features
/// </summary>
[Authorize]
public class ExamHub : Hub
{
    private readonly ILogger<ExamHub> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ExamHub(ILogger<ExamHub> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Join exam session group
    /// </summary>
    public async Task JoinExamSession(string examSessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"exam_{examSessionId}");
        await Clients.OthersInGroup($"exam_{examSessionId}")
            .SendAsync("StudentJoined", new { UserId = Context.UserIdentifier, JoinedAt = DateTime.UtcNow });
        
        _logger.LogInformation("User {UserId} joined exam session {ExamSessionId}", Context.UserIdentifier, examSessionId);
    }

    /// <summary>
    /// Leave exam session group
    /// </summary>
    public async Task LeaveExamSession(string examSessionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"exam_{examSessionId}");
        await Clients.OthersInGroup($"exam_{examSessionId}")
            .SendAsync("StudentLeft", new { UserId = Context.UserIdentifier, LeftAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Submit answer in real-time
    /// </summary>
    public async Task SubmitAnswer(AnswerSubmissionDto answer)
    {
        await Clients.Group($"exam_{answer.ExamSessionId}")
            .SendAsync("AnswerSubmitted", new { 
                UserId = Context.UserIdentifier, 
                QuestionId = answer.QuestionId, 
                SubmittedAt = DateTime.UtcNow 
            });
    }

    /// <summary>
    /// Request help during exam
    /// </summary>
    public async Task RequestHelp(HelpRequestDto helpRequest)
    {
        await Clients.Group($"exam_{helpRequest.ExamSessionId}")
            .SendAsync("HelpRequested", new { 
                UserId = Context.UserIdentifier, 
                QuestionId = helpRequest.QuestionId, 
                Message = helpRequest.Message,
                RequestedAt = DateTime.UtcNow 
            });
    }

    /// <summary>
    /// Send proctoring alert
    /// </summary>
    public async Task SendProctoringAlert(ProctoringAlertDto alert)
    {
        await Clients.Group($"exam_{alert.ExamSessionId}")
            .SendAsync("ProctoringAlert", new { 
                UserId = Context.UserIdentifier, 
                AlertType = alert.AlertType, 
                Message = alert.Message,
                Timestamp = DateTime.UtcNow 
            });
    }

    /// <summary>
    /// Update exam timer
    /// </summary>
    public async Task UpdateTimer(string examSessionId, int remainingSeconds)
    {
        await Clients.Group($"exam_{examSessionId}")
            .SendAsync("TimerUpdated", new { RemainingSeconds = remainingSeconds });
    }

    /// <summary>
    /// Auto-submit exam when time expires
    /// </summary>
    public async Task AutoSubmitExam(string examSessionId)
    {
        await Clients.Group($"exam_{examSessionId}")
            .SendAsync("ExamAutoSubmitted", new { 
                ExamSessionId = examSessionId, 
                SubmittedAt = DateTime.UtcNow 
            });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Remove from all exam groups
        // Implementation depends on your tracking strategy
        await base.OnDisconnectedAsync(exception);
    }
}

// DTOs for real-time communication
public class AnswerSubmissionDto
{
    public string ExamSessionId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string SelectedOptionId { get; set; } = string.Empty;
}

public class HelpRequestDto
{
    public string ExamSessionId { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ProctoringAlertDto
{
    public string ExamSessionId { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
*/
