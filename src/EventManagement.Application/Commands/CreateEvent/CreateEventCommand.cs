// EventManagement.Application/Commands/CreateEventCommand.cs

namespace EventManagement.Application.Commands.CreateEvent;

public sealed record CreateEventCommand(
    string Titre,
    string Description,
    string PersonneImpliquee);