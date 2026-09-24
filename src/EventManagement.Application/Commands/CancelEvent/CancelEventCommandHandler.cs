// src/EventManagement.Application/Commands/CancelEvent/CancelEventCommandHandler.cs

namespace EventManagement.Application.Commands.CancelEvent;

using EventManagement.Application.Exceptions;
using EventManagement.Application.Interfaces;

public sealed class CancelEventCommandHandler
{
    private readonly IEvenementRepository _repository;

    public CancelEventCommandHandler(IEvenementRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(CancelEventCommand command, CancellationToken ct)
    {
        var evt = await _repository.GetByIdWithCommentsAsync(command.Id, ct)
            ?? throw new NotFoundException($"Evenement {command.Id} not found.");

        var now = DateTimeOffset.UtcNow;

        evt.Annuler(now);

        await _repository.SaveChangesAsync(ct);
    }
}