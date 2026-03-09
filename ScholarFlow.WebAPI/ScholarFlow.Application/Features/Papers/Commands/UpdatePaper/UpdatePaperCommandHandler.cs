using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Application.DTOs;
using ScholarFlow.Domain.Interfaces;
using ScholarFlow.Domain.Entities;

namespace ScholarFlow.Application.Features.Papers.Commands.UpdatePaper;

public class UpdatePaperCommandHandler : IRequestHandler<UpdatePaperCommand, Result<PaperDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdatePaperCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaperDto>> Handle(UpdatePaperCommand request, CancellationToken cancellationToken)
    {
        // Get the paper to update
        var paper = await _context.Papers
            .Include(p => p.Subject)
            .Include(p => p.Creator)
            .Include(p => p.Questions)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (paper == null)
        {
            return Result<PaperDto>.Failure("Paper not found");
        }

        // Update paper properties
        paper.SubjectId = request.SubjectId;
        paper.Year = request.Year;
        paper.Type = request.Type;
        paper.Title = request.Title;
        paper.TimeLimit = request.TimeLimit;
        paper.UpdatedAt = DateTime.UtcNow;

        // Validate that the subject exists
        var subjectExists = await _context.Subjects
            .AnyAsync(s => s.Id == request.SubjectId, cancellationToken);
        
        if (!subjectExists)
        {
            return Result<PaperDto>.Failure("Subject not found");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            // Return updated DTO
            var updatedDto = new PaperDto
            {
                Id = paper.Id,
                SubjectId = paper.SubjectId,
                SubjectName = paper.Subject?.Name ?? "Unknown",
                Year = paper.Year,
                Type = paper.Type.ToString(),
                Title = paper.Title,
                TimeLimit = paper.TimeLimit,
                CreatedByTeacher = paper.CreatedByTeacher,
                CreatedByTeacherName = paper.Creator?.UserName ?? "Unknown",
                QuestionCount = paper.Questions.Count
            };

            return Result<PaperDto>.Success(updatedDto);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result<PaperDto>.Failure("The paper has been modified by another user. Please refresh and try again.");
        }
        catch (Exception ex)
        {
            return Result<PaperDto>.Failure($"Failed to update paper: {ex.Message}");
        }
    }
}
