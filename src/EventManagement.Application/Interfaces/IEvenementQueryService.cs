// EventManagement.Application/Interfaces/IEventQueryService.cs

using EventManagement.Application.Common;
using EventManagement.Application.Queries.GetEventsPaged;
using EventManagement.Application.ReadModels;


namespace EventManagement.Application.Interfaces;
/*
 * Retourne des ReadModelsoptimisations pour l'affichage.
 * Pas de référence aux entités Domaine dans les retours (séparation claire). 
 */
public interface IEvenementQueryService
{
   // Task<EventListReadModel> GetEventsPagedAsync(int page,int pageSize, CancellationToken cancellationToken = default);

    Task<PagedResult<EventListItemReadModel>> GetEventsPagedAsync(GetEventsPagedQuery query, CancellationToken ct = default);

    Task<EventDetailsReadModel?> GetEventByIdAsync(Guid id,CancellationToken cancellationToken = default);

    Task<CommentListReadModel> GetCommentsAsync(Guid eventId, CancellationToken cancellationToken = default);
}