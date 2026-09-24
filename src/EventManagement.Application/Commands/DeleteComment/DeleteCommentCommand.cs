// src/EventManagement.Application/Commands/DeleteComment/DeleteCommentCommand.cs

namespace EventManagement.Application.Commands.DeleteComment;

public sealed record DeleteCommentCommand(Guid EventId, Guid CommentId);