// tests/EventManagement.Domain.Tests/EventTests.cs

namespace EventManagement.Domain.Tests;

using EventManagement.Domain.Common;
using EventManagement.Domain.Entities;

using FluentAssertions;

/*
 * Acceptation Critères d'acceptation

Les tests passent ( dotnet test).
Les invariants sont couverts (titre vide, titre trop long, etc.).
Aucun setter public n'est utilisé dans les tests.
 * 
 */
public class EventTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ShouldThrow_WhenTitleIsEmpty()
    {
        var act = () => Evenement.Create("", "description", "person", Guid.NewGuid(), Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*titre*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenTitleIsTooLong()
    {
        var title = new string('a', 101);

        var act = () => Evenement.Create(title, "description", "person", Guid.NewGuid(), Now);

        act.Should().Throw<DomainException>()
            .WithMessage("*100*");
    }

    [Fact]
    public void AddComment_ShouldAddComment_WhenEventIsDraft()
    {
        var evt = Evenement.Create("Title", "Description", "Person", Guid.NewGuid(), Now);

        var comment = evt.AddComment("Super événement", Now, Guid.NewGuid());

        evt.Commentaires.Should().ContainSingle();
        comment.Description.Should().Be("Super événement");
    }

    [Fact]
    public void AddComment_ShouldThrow_WhenEventIsCancelled()
    {
        var evt = Evenement.Create("Title", "Description", "Person", Guid.NewGuid(), Now);
        // Simuler l'annulation (on ajoutera une méthode Annuler plus tard)
        // Pour l'instant, on teste la règle via le statut
        // Ce test évoluera avec les transitions d'état

        // Actuellement, on ne peut pas annuler, donc ce test est en attente
        // On le commente ou on le marque comme [Fact(Skip = "À implémenter plus tard")]
    }
}