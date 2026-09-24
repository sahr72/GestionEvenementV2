// src/EventManagement.Application/Commands/ConfirmEvent/ConfirmEventCommandHandler.cs

namespace EventManagement.Application.Commands.ConfirmEvent;

using EventManagement.Application.Exceptions;
using EventManagement.Application.Interfaces;

public sealed class ConfirmEventCommandHandler
{
    private readonly IEvenementRepository _repository;

    public ConfirmEventCommandHandler(IEvenementRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(ConfirmEventCommand command, CancellationToken ct)
    {
        var evt = await _repository.GetByIdWithCommentsAsync(command.Id, ct)
            ?? throw new NotFoundException($"Event {command.Id} not found.");

        var now = DateTimeOffset.UtcNow;

        evt.Confirmer(now);

        await _repository.SaveChangesAsync(ct);
    }
}