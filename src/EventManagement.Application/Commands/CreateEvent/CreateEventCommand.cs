// EventManagement.Application/Commands/CreateEventCommand.cs

namespace EventManagement.Application.Commands;

public sealed record CreateEventCommand(
    string Titre,
    string Description,
    string PersonneImpliquee);