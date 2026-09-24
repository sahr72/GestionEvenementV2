// EventManagement.Application/Queries/GetEventsPagedQueryHandler.cs

using EventManagement.Application.Common;
using EventManagement.Application.Interfaces;
using EventManagement.Application.Queries.GetEventsPaged;
using EventManagement.Application.ReadModels;

namespace EventManagement.Application.Queries.GetEventsPaged;
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

    public Task<PagedResult<EventListItemReadModel>> Handle(GetEventsPagedQuery query, CancellationToken cancellationToken = default)
    {
        //return _eventQueryService.GetEventsPagedAsync(
        //    page: query.Page,
        //    pageSize: query.PageSize,
        //    status:query.Statut,
        //    cancellationToken);
        return _eventQueryService.GetEventsPagedAsync(query, cancellationToken);
    }
}