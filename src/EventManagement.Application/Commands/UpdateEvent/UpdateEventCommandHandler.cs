// src/EventManagement.Application/Commands/UpdateEvent/UpdateEventCommandHandler.cs

namespace EventManagement.Application.Commands.UpdateEvent;
using EventManagement.Application.Exceptions;
using EventManagement.Application.Interfaces;

//UpdateEventCommandHandler travaille avec l'agrégat retourné par le repository,
//mais ne référence pas directement son type.
public sealed class UpdateEventCommandHandler
{
    private readonly IEvenementRepository _repository;

    public UpdateEventCommandHandler(IEvenementRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateEventCommand command, CancellationToken ct)
    {
        var aggregate = await _repository.GetByIdWithCommentsAsync(command.Id, ct);
        if (aggregate is null)
            throw new NotFoundException($"Événement {command.Id} introuvable.");

        var now = DateTimeOffset.UtcNow; // À remplacer par TimeProvider

        aggregate.Update(command.Titre, command.Description, command.personneImpliquee, now);

        await _repository.SaveChangesAsync(ct);
    }
}