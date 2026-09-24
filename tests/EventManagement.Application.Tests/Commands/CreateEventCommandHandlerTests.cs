// tests/EventManagement.Application.Tests/Commands/CreateEventCommandHandlerTests.cs

namespace EventManagement.Application.Tests.Commands;

using EventManagement.Application.Commands;
using EventManagement.Application.Commands.CreateEvent;
using EventManagement.Application.Interfaces;
using EventManagement.Domain.Entities;
using FluentAssertions;
using NSubstitute;
/*
 * Critères d'acceptation
Les tests applicatifs passent.
Les mocks sont utilisés pour isoler le domaine.
Les cas d'erreur (ex: événement introuvable) sont testés.
 * */
public class CreateEventCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateEvent()
    {
        var repository = Substitute.For<IEvenementRepository>();
        var handler = new CreateEventCommandHandler(repository);

        var command = new CreateEventCommand("Titre", "Description", "PersonneImpliquee");

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
        await repository.Received(1).AddAsync(Arg.Any<Evenement>(), Arg.Any<CancellationToken>());
        await repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}