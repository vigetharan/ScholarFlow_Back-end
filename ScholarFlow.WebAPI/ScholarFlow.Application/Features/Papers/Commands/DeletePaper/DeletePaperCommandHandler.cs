using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Papers.Commands.DeletePaper;

public class DeletePaperCommandHandler : IRequestHandler<DeletePaperCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DeletePaperCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DeletePaperCommand request, CancellationToken cancellationToken)
    {
        var paper = await _context.Papers
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (paper == null)
        {
            return Result<bool>.Failure("Paper not found");
        }

        // Check permissions
        if (request.UserRole == "ADMIN")
        {
            // Admin can delete any paper
        }
        else if (request.UserRole == "TEACHER")
        {
            // Teacher can only delete their own papers
            if (paper.CreatedByTeacher != request.UserId)
            {
                return Result<bool>.Failure("You can only delete papers you created");
            }
        }
        else
        {
            return Result<bool>.Failure("Unauthorized");
        }

        paper.IsDeleted = true;
        paper.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
