
namespace EventManagement.Domain.Entities;
using EventManagement.Domain.Common;
using EventManagement.Domain.SmartEnums;
using System.Runtime.ConstrainedExecution;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
/*
 * 3. Vérification des principes DDD
    3.1 Racine agrégée
    Evenementest l'Aggregate Root.
    Les modifications des commentaires passent par Evenement.AddComment/ RemoveComment.
    Evenementcontrôle l'ajout/suppression de commentaires.

    3.2 Encapsulation
    Pas de public set;sur les propriétés métier.
    Les invariants sont protégés par des méthodes ( SetTitre, SetDescription).

    3.3 Technologie d'isolation
    EF Core, ASP.NET Core, HTTP, JSON, etc.
    Le constructeur privé sans paramètre est autorisé pour EF Core, mais le Domaine ignore complètement comment il est utilisé.

    4. Autocritique
    Simplicité : On a deux entités simples, avec des règles métier claires. Pas de sur-ingénierie.
    Protection des invariants : Les règles « titre obligatoire », « titre ≤ 100 caractères », « description obligatoire » sont bien dans le Domaine.
    Préparation pour EF Core : Les constructeurs privés permettent à EF Core de matérialiser les objets, mais le Domaine reste ignorant de la persistance.
pour toute entit/ n/c/ssitant une validation, il faudrait passer par une methode privee pour proteger l'invariant
 */


/* 
 Acceptation Critères d'acceptation:
 Evenement et Commentaire sont dans EventManagement.Domain.
 Aucun setter public sur les propriétés métier.
 Les invariants (titre obligatoire, titre ≤ 100 caractères, description obligatoire, personne obligatoire) sont protégés.
 EventStatus est une Smart Enum avec transitions contrôlées.
 dotnet buildréussi.
 */
public sealed class Evenement
{
    private readonly List<Commentaire> _commentaires = new();

    private Evenement() { } // Constructeur privé pour EF Core

    /* On evite le constructeur et on previlegie une methode Factory Create
    public Evenement(Guid id,string titre,string description,string personneImpliquee)
    {
        Id = id;
        SetTitre(titre);
        Description = description;
        PersonneImpliquee = personneImpliquee;
    }
    */

    /* Factory method(Create) pour la création “métier”:
        Utilisez une méthode Create statique(ou une factory séparée) pour :
        créer un nouvel agrégat avec un ID généré,
        valider les invariants,
        initialiser les valeurs par défaut(Draft, CreatedAtUtc).
    */
    public static Evenement Create(string titre,string description,string personneImpliquee,DateTimeOffset now)
    {
        Valider(titre, description, personneImpliquee);

        return new Evenement
        {
            Id = Guid.NewGuid(),
            Titre = titre.Trim(),
            Description = description.Trim(),
            PersonneImpliquee = personneImpliquee.Trim(),
            Statut = EventStatus.Draft,
            DateCreationUtc = now
        };
    }



    public Guid Id { get; private set; }
    public string Titre { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string PersonneImpliquee { get; private set; } = string.Empty;
    public EventStatus Statut { get; private set; } = EventStatus.Draft;
    public DateTimeOffset DateCreationUtc { get; private set; }
    public DateTimeOffset? DateMiseAjourUtc { get; private set; }
    public IReadOnlyCollection<Commentaire> Commentaires => _commentaires.AsReadOnly();

    public void Update(string titre, string description, string personneImpliquee, DateTimeOffset? now)
    {
        VerifierSiModifiable();
        Valider(titre, description, personneImpliquee);

        Titre=titre.Trim();
        Description = description.Trim();
        PersonneImpliquee = personneImpliquee;
        DateMiseAjourUtc = now ?? DateTimeOffset.UtcNow;
    }

    /*L’événement (aggregat root) crée lui-même le commentaire(new Comment(...)).
    Il contrôle totalement la création du commentaire(ID, EventId, date).
    Le commentaire est immédiatement ajouté à la collection interne.
    Le retour est le commentaire créé.
    */
    public Commentaire AddComment(string description, DateTimeOffset now)
    {
        VerifierSicommentable();
        var commentaire = new Commentaire(Guid.NewGuid(), description, now, Id);

        _commentaires.Add(commentaire);
        return commentaire;
    }

    /*Avant application des règles de gestion
   
    public void RemoveComment(Guid commentId)
    {
        var comment = _commentaires.FirstOrDefault(c => c.Id == commentId);
        if (comment is null)
            throw new DomainException("Commentaire introuvable.");

        _commentaires.Remove(comment);
    }
    */
    // src/EventManagement.Domain/Entities/Event.cs

    public void RemoveComment(Guid commentId)
    {
        if (Statut == EventStatus.Cancelled || Statut == EventStatus.Closed)
            throw new DomainException("Cannot remove comments from a cancelled or closed event.");

        var comment = _commentaires.FirstOrDefault(c => c.Id == commentId);
        if (comment is null)
            throw new DomainException($"Comment {commentId} not found.");

        _commentaires.Remove(comment);
    }

    private void VerifierSiModifiable()
    {
        if (Statut == EventStatus.Cancelled || Statut == EventStatus.Closed)
            throw new DomainException("L'événement ne peut plus être modifié.");
    }
    
    private void VerifierSicommentable()
    {
        if (Statut == EventStatus.Cancelled || Statut == EventStatus.Closed)
            throw new DomainException("Impossible d'ajouter un commentaire sur cet événement.");
    }
    
    //Proteger les invariants
    private static void Valider(string titre, string description, string personName)
    {
        if (string.IsNullOrWhiteSpace(titre))
            throw new DomainException("Le titre est obligatoire.");

        if (titre.Trim().Length > 100)
            throw new DomainException("Le titre ne peut pas dépasser 100 caractères.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("La description est obligatoire.");

        if (string.IsNullOrWhiteSpace(personName))
            throw new DomainException("La personne impliquée est obligatoire.");
    }
    // src/EventManagement.Domain/Entities/Event.cs

    public void Annuler(DateTimeOffset now)
    {
        if (!Statut.CanTransitionTo(EventStatus.Cancelled))
            throw new DomainException($"Cannot cancel event from status '{Statut.Name}'.");

        Statut = EventStatus.Cancelled;
        DateMiseAjourUtc = now;
    }
    // src/EventManagement.Domain/Entities/Event.cs

    public void Fermer(DateTimeOffset now)
    {
        if (!Statut.CanTransitionTo(EventStatus.Closed))
            throw new DomainException($"Cannot close event from status '{Statut.Name}'.");

        Statut = EventStatus.Closed;
        DateMiseAjourUtc = now;
    }
    // src/EventManagement.Domain/Entities/Event.cs

    public void Confirmer(DateTimeOffset now)
    {
        if (!Statut.CanTransitionTo(EventStatus.Confirmed))
            throw new DomainException($"Cannot confirm event from status '{Statut.Name}'.");

        Statut = EventStatus.Confirmed;
        DateMiseAjourUtc = now;
    }
    // src/EventManagement.Domain/Entities/Event.cs

    public void UpdateComment(Guid commentId, string description, DateTimeOffset now)
    {
        if (Statut == EventStatus.Cancelled || Statut == EventStatus.Closed)
            throw new DomainException("Cannot update comments on a cancelled or closed event.");

        var comment = _commentaires.FirstOrDefault(c => c.Id == commentId)
            ?? throw new DomainException($"Comment {commentId} not found.");

        comment.MettreAJour(description, now);
    }
}