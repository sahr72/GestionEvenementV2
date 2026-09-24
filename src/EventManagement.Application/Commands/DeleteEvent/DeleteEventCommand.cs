// src/EventManagement.Application/Commands/DeleteEvent/DeleteEventCommand.cs

namespace EventManagement.Application.Commands.DeleteEvent;

public sealed record DeleteEventCommand(Guid Id);