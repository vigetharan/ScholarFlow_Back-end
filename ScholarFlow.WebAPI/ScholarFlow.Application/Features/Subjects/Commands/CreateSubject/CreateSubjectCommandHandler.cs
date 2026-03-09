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
        // Check for duplicate subject name
        var existingSubject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Name.ToLower() == request.Name.ToLower(), cancellationToken);

        if (existingSubject != null)
        {
            return Result<SubjectDto>.Failure($"Subject '{request.Name}' already exists");
        }

        // Create subject (independent of streams now)
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync(cancellationToken);

        // Map to DTO (no streams initially)
        var dto = new SubjectDto
        {
            Id = subject.Id,
            Name = subject.Name,
            StreamNames = new List<string>(),
            StreamIds = new List<Guid>()
        };

        return Result<SubjectDto>.Success(dto);
    }
}
