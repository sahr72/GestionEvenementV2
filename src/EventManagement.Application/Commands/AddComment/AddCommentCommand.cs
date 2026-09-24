// src/EventManagement.Application/Commands/AddComment/AddCommentCommand.cs

namespace EventManagement.Application.Commands.AddComment;

public sealed record AddCommentCommand(
    Guid EventId,
    string Description);