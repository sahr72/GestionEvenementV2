// src/EventManagement.Application/Commands/DeleteComment/DeleteCommentCommandHandler.cs

namespace EventManagement.Application.Commands.DeleteComment;

using EventManagement.Application.Exceptions;
using EventManagement.Application.Interfaces;

public sealed class DeleteCommentCommandHandler
{
    private readonly IEvenementRepository _repository;

    public DeleteCommentCommandHandler(IEvenementRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(DeleteCommentCommand command, CancellationToken ct)
    {
        var evt = await _repository.GetByIdWithCommentsAsync(command.EventId, ct)
            ?? throw new NotFoundException($"Event {command.EventId} not found.");

        evt.RemoveComment(command.CommentId);

        await _repository.SaveChangesAsync(ct);
    }
}