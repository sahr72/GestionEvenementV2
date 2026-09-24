// src/EventManagement.Application/Commands/AddComment/AddCommentCommandHandler.cs

namespace EventManagement.Application.Commands.AddComment;

using EventManagement.Application.Exceptions;
using EventManagement.Application.Interfaces;

public sealed class AddCommentCommandHandler
{
    private readonly IEvenementRepository _eventRepository;

    public AddCommentCommandHandler(IEvenementRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Guid> Handle(AddCommentCommand command, CancellationToken ct)
    {
        var evt = await _eventRepository.GetByIdWithCommentsAsync(command.EventId, ct)
            ?? throw new NotFoundException($"Event {command.EventId} not found.");

        var now = DateTimeOffset.UtcNow;
        var comment = evt.AddComment(command.Description, now);

        await _eventRepository.SaveChangesAsync(ct);

        return comment.Id;
    }
}