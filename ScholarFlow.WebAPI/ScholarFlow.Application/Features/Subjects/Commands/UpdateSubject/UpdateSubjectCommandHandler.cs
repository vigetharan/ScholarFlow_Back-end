using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Entities;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Subjects.Commands.UpdateSubject;

/// <summary>
/// Handler for UpdateSubjectCommand
/// </summary>
public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, Result<SubjectDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<SubjectDto>> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        // Find subject
        var subject = await _context.Subjects
            .Include(s => s.Stream)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subject == null)
        {
            return Result<SubjectDto>.Failure("Subject not found");
        }

        // Check if stream exists
        var stream = await _context.Streams
            .FirstOrDefaultAsync(s => s.Id == request.StreamId, cancellationToken);

        if (stream == null)
        {
            return Result<SubjectDto>.Failure("Stream not found");
        }

        // Check for duplicate name (exclude current subject)
        var existingSubject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Name.ToLower() == request.Name.ToLower() && s.Id != request.Id, 
                                cancellationToken);

        if (existingSubject != null)
        {
            return Result<SubjectDto>.Failure($"Subject '{request.Name}' already exists");
        }

        // Update subject
        subject.Name = request.Name;
        subject.StreamId = request.StreamId;

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
