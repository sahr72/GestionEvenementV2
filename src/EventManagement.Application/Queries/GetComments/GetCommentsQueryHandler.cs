// src/EventManagement.Application/Queries/GetComments/GetCommentsQueryHandler.cs

namespace EventManagement.Application.Queries.GetComments;

using EventManagement.Application.Interfaces;
using EventManagement.Application.ReadModels;

public sealed class GetCommentsQueryHandler
{
    private readonly ICommentaireQueryService _queryService;

    public GetCommentsQueryHandler(ICommentaireQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<IReadOnlyList<CommentReadModel>> Handle(
        GetCommentsQuery query,
        CancellationToken ct)
    {
        return await _queryService.GetCommentsByEventIdAsync(query.EvenementId, ct);
    }
}