// EventManagement.Application/Queries/GetEventsPagedQueryHandler.cs

using EventManagement.Application.Interfaces;
using EventManagement.Application.ReadModels;

namespace EventManagement.Application.Queries;
/*
 * Le Handler délègue complètement à IEventQueryService.
   Les projections et AsNoTracking()seront dans l'implémentation Infrastructure. 
 */
public sealed class GetEventsPagedQueryHandler
{
    private readonly IEvenementQueryService _eventQueryService;

    public GetEventsPagedQueryHandler(IEvenementQueryService eventQueryService)
    {
        _eventQueryService = eventQueryService;
    }

    public Task<EventListReadModel> Handle(GetEventsPagedQuery query, CancellationToken cancellationToken = default)
    {
        return _eventQueryService.GetEventsPagedAsync(
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken);
    }
}