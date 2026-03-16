using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScholarFlow.Application.Features.Students.Commands.ConnectTeacher;
using ScholarFlow.Application.Features.Students.Commands.CreateProfile;
using ScholarFlow.Application.Features.Students.Commands.DisconnectTeacher;
using ScholarFlow.Application.Features.Students.Commands.UpdateProfile;
using ScholarFlow.Application.Features.Students.Queries.GetConnectedTeachers;
using ScholarFlow.Application.Features.Students.Queries.GetProfile;

namespace ScholarFlow.WebAPI.Controllers;

/// <summary>
/// Student Profile API Controller
/// </summary>
[ApiController]
[Route("api/students")]
[Authorize(Roles = "STUDENT,Student")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StudentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create student profile
    /// </summary>
    [HttpPost("profile")]
    public async Task<IActionResult> CreateProfile([FromBody] CreateStudentProfileCommand command, CancellationToken cancellationToken)
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
    /// Get current student's profile
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        // Extract UserId from JWT token
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        // Create query with UserId
        var query = new GetStudentProfileQuery { UserId = userId };

        // Delegate to handler
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Update student profile
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentProfileCommand command, CancellationToken cancellationToken)
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
    /// Submit access request to a teacher via teacher code
    /// </summary>
    [HttpPost("teachers/connect")]
    public async Task<IActionResult> ConnectTeacher([FromBody] ConnectTeacherRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        var command = new ConnectTeacherCommand
        {
            StudentUserId = userId,
            TeacherCode = request.TeacherCode ?? string.Empty,
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Get all teachers connected to current student
    /// </summary>
    [HttpGet("teachers")]
    public async Task<IActionResult> GetConnectedTeachers(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        var query = new GetConnectedTeachersQuery { StudentUserId = userId };
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Disconnect teacher from current student
    /// </summary>
    [HttpDelete("teachers/{teacherUserId:guid}")]
    public async Task<IActionResult> DisconnectTeacher(Guid teacherUserId, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        var command = new DisconnectTeacherCommand
        {
            StudentUserId = userId,
            TeacherUserId = teacherUserId,
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return NoContent();
    }
}

public class ConnectTeacherRequest
{
    public string? TeacherCode { get; set; }
}
