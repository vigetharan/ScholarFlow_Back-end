using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScholarFlow.Application.Features.Teachers.Commands.CreateProfile;
using ScholarFlow.Application.Features.Teachers.Commands.ReviewRegistration;
using ScholarFlow.Application.Features.Teachers.Commands.ReviewStudentAccess;
using ScholarFlow.Application.Features.Teachers.Commands.UpdateProfile;
using ScholarFlow.Application.Features.Teachers.Queries.GetApprovalRequests;
using ScholarFlow.Application.Features.Teachers.Queries.GetProfile;
using ScholarFlow.Application.Features.Teachers.Queries.GetStudentAccessRequests;
using ScholarFlow.Domain.Enums;

namespace ScholarFlow.WebAPI.Controllers;

/// <summary>
/// Teacher Profile API Controller
/// </summary>
[ApiController]
[Route("api/teachers")]
[Authorize(Roles = "TEACHER,ADMIN")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TeachersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create teacher profile
    /// </summary>
    [HttpPost("profile")]
    [Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateTeacherProfileCommand command, CancellationToken cancellationToken)
    {
        // Extract UserId from JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Set UserId in command
        command.UserId = userId;

        // Delegate to handler
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get current teacher's profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        // Extract UserId from JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Create query with UserId
        var query = new GetTeacherProfileQuery { UserId = userId };

        // Delegate to handler
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Update teacher profile
    /// </summary>
    [HttpPut("profile")]
    [Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateTeacherProfileCommand command, CancellationToken cancellationToken)
    {
        // Extract UserId from JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Set UserId in command
        command.UserId = userId;

        // Delegate to handler
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get teacher approval requests (Admin only)
    /// </summary>
    [HttpGet("admin/requests")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> GetApprovalRequests([FromQuery] TeacherRegistrationStatus? status, CancellationToken cancellationToken)
    {
        var query = new GetTeacherApprovalRequestsQuery { Status = status };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Review a teacher registration request (Admin only)
    /// </summary>
    [HttpPut("admin/{teacherUserId:guid}/status")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> ReviewRegistration(Guid teacherUserId, [FromBody] ReviewTeacherRegistrationRequest request, CancellationToken cancellationToken)
    {
        var reviewerClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(reviewerClaim) || !Guid.TryParse(reviewerClaim, out var reviewerId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        var command = new ReviewTeacherRegistrationCommand
        {
            TeacherUserId = teacherUserId,
            ReviewedById = reviewerId,
            Status = request.Status,
            RejectionReason = request.RejectionReason
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get student access requests for current teacher
    /// </summary>
    [HttpGet("students/requests")]
    [Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> GetStudentAccessRequests([FromQuery] StudentTeacherConnectionStatus? status, CancellationToken cancellationToken)
    {
        var teacherClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(teacherClaim) || !Guid.TryParse(teacherClaim, out var teacherUserId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        var query = new GetStudentAccessRequestsQuery
        {
            TeacherUserId = teacherUserId,
            Status = status
        };

        var result = await _mediator.Send(query, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Approve or reject a student's paper access request
    /// </summary>
    [HttpPut("students/requests/{studentUserId:guid}/status")]
    [Authorize(Roles = "TEACHER")]
    public async Task<IActionResult> ReviewStudentAccess(Guid studentUserId, [FromBody] ReviewStudentAccessRequest request, CancellationToken cancellationToken)
    {
        var teacherClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(teacherClaim) || !Guid.TryParse(teacherClaim, out var teacherUserId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        var command = new ReviewStudentAccessCommand
        {
            TeacherUserId = teacherUserId,
            StudentUserId = studentUserId,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }
}

public class ReviewTeacherRegistrationRequest
{
    public TeacherRegistrationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
}

public class ReviewStudentAccessRequest
{
    public StudentTeacherConnectionStatus Status { get; set; }
}
