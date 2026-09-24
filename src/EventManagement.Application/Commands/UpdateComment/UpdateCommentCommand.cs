// src/EventManagement.Application/Commands/UpdateComment/UpdateCommentCommand.cs

namespace EventManagement.Application.Commands.UpdateComment;

public sealed record UpdateCommentCommand(
    Guid EventId,
    Guid CommentId,
    string Description);