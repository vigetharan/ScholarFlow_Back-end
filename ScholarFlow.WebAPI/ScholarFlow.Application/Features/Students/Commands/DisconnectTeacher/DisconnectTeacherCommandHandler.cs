using MediatR;
using Microsoft.EntityFrameworkCore;
using ScholarFlow.Application.Common.Models;
using ScholarFlow.Domain.Interfaces;

namespace ScholarFlow.Application.Features.Students.Commands.DisconnectTeacher;

public class DisconnectTeacherCommandHandler : IRequestHandler<DisconnectTeacherCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public DisconnectTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> Handle(DisconnectTeacherCommand request, CancellationToken cancellationToken)
    {
        var connection = await _context.StudentTeacherConnections
            .FirstOrDefaultAsync(
                c => c.StudentUserId == request.StudentUserId && c.TeacherUserId == request.TeacherUserId,
                cancellationToken);

        if (connection == null)
        {
            return Result<bool>.Success(true);
        }

        _context.StudentTeacherConnections.Remove(connection);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
