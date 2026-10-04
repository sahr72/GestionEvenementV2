// EventManagement.Domain/Entities/Commentaire.cs

using EventManagement.Domain.Common;

namespace EventManagement.Domain.Entities;

public sealed class Commentaire
{
    private Commentaire() { } // EF Core

    public Commentaire(Guid id, string description, DateTimeOffset dateCreation, Guid eventId, Guid auteurId)
    {
        Id = id;
        SetDescription(description);
        AuteurId = auteurId != Guid.Empty ? auteurId
            : throw new DomainException("AuteurId est obligatoire.");
        DateCreationUtc = dateCreation;
        EventId = eventId != Guid.Empty ? eventId
            : throw new DomainException("EventId est obligatoire.");
    }

    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset DateCreationUtc { get; private set; }
    public DateTimeOffset? DateMiseAJourUtc { get; private set; }
    public Guid EventId { get; private set; }
    public Guid AuteurId { get; private set; }

    public void MettreAJour(string description, DateTimeOffset now)
    {
        SetDescription(description);
        DateMiseAJourUtc = now;
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("La description est obligatoire.");

        Description = description.Trim();
    }
}