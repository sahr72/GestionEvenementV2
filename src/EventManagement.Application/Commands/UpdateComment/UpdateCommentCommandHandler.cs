// src/EventManagement.Application/Commands/UpdateComment/UpdateCommentCommandHandler.cs

namespace EventManagement.Application.Commands.UpdateComment;

using EventManagement.Application.Exceptions;
using EventManagement.Application.Interfaces;

public sealed class UpdateCommentCommandHandler
{
    private readonly IEvenementRepository _repository;

    public UpdateCommentCommandHandler(IEvenementRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateCommentCommand command, CancellationToken ct)
    {
        var evt = await _repository.GetByIdWithCommentsAsync(command.EventId, ct)
            ?? throw new NotFoundException($"Event {command.EventId} not found.");

        var now = DateTimeOffset.UtcNow;

        evt.UpdateComment(command.CommentId, command.Description, now);

        await _repository.SaveChangesAsync(ct);
    }
}