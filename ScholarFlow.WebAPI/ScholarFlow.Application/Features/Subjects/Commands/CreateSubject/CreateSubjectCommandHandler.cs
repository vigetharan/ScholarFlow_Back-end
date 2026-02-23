using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Subjects.Commands.CreateSubject;

/// <summary>
/// Handler for CreateSubjectCommand
/// </summary>
public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, Result<SubjectDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateSubjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SubjectDto>> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        // Check if stream exists
        var stream = await _context.Streams
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            return Result<SubjectDto>.Failure("Stream not found");
        }

        // Check for duplicate subject name
        var existingSubject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingSubject != null)
        {
            return Result<SubjectDto>.Failure($"Subject '{request.Name}' already exists");
        }

        // Create subject
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StreamId = request.StreamId
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO
        var dto = new SubjectDto
        {
            Id = subject.Id,
            Name = subject.Name,
            StreamId = request.StreamId,
            StreamName = stream.Name
        };

        return Result<SubjectDto>.Success(dto);
    }
}
