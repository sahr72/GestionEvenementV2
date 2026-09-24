// EventManagement.Application/Commands/CreateEventCommandHandler.cs

using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;

namespace EventManagement.Application.Commands.CreateEvent;
/*
 * Le Handler utilise le Domaine ( Evenementconstructeur) pour créer l'entité.
   Il délègue la persistance à IEvenementRepository.
   Pas de métier logique dans le Handler.
 */
public sealed class CreateEventCommandHandler
{
    private readonly IEvenementRepository _eventRepository;

    public CreateEventCommandHandler(IEvenementRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Guid> Handle(CreateEventCommand command, CancellationToken ct = default)
    {
        // La validation Application (FluentValidation) sera ajoutée plus tard
        var now = DateTimeOffset.UtcNow; // À remplacer par TimeProvider plus tard
        var aggregat =  Evenement.Create(titre: command.Titre,
                                        description: command.Description,
                                        personneImpliquee: command.PersonneImpliquee, 
                                        now);

        await _eventRepository.AddAsync(aggregat, ct);
        await _eventRepository.SaveChangesAsync(ct);
        return aggregat.Id;
    }
}