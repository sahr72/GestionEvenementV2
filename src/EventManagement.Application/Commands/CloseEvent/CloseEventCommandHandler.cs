// src/EventManagement.Application/Commands/CloseEvent/CloseEventCommandHandler.cs

namespace EventManagement.Application.Commands.CloseEvent;

using EventManagement.Application.Exceptions;
using EventManagement.Application.Interfaces;

public sealed class CloseEventCommandHandler
{
    private readonly IEvenementRepository _repository;

    public CloseEventCommandHandler(IEvenementRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CloseEventCommand command, CancellationToken ct)
    {
        var evt = await _repository.GetByIdWithCommentsAsync(command.Id, ct)
            ?? throw new NotFoundException($"Event {command.Id} not found.");

        var now = DateTimeOffset.UtcNow;

        evt.Fermer(now);

        await _repository.SaveChangesAsync(ct);
    }
}