// src/EventManagement.Application/Commands/UpdateEvent/UpdateEventCommand.cs

namespace EventManagement.Application.Commands.UpdateEvent;

public sealed record UpdateEventCommand(
    Guid Id,
    string Titre,
    string Description,
    string PersonneImpliquee);