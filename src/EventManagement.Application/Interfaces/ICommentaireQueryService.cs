// src/EventManagement.Application/Interfaces/ICommentaireQueryService.cs

namespace EventManagement.Application.Interfaces;

using EventManagement.Application.ReadModels;

public interface ICommentaireQueryService
{
    Task<IReadOnlyList<CommentReadModel>> GetCommentsByEventIdAsync(
        Guid eventId,
        CancellationToken ct);

    Task<CommentReadModel?> GetByIdAsync(
        Guid commentId,
        CancellationToken ct);
}