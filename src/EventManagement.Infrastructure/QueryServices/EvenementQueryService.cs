// src_EventManagement.Infrastructure/QueryServices/EventQueryService.cs

namespace EventManagement.Infrastructure.QueryServices;

using global::EventManagement.Application.Common;
using global::EventManagement.Application.Interfaces;
using global::EventManagement.Application.Queries.GetEventsPaged;
using global::EventManagement.Application.ReadModels;
using global::EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/*
 * liste paginée → EventListItemReadModel
détail → EventDetailsReadModel
ne pas projeter la liste vers EventDetailsReadModel
PagedResult<T> doit contenir Items, TotalCount, PageNumber, PageSize
attention à EventStatus : c'est une Smart Enum, donc e.Statut.Name dépend de la manière dont EF Core est configuré.
 * 
 * */
public sealed class EvenementQueryService : IEvenementQueryService
{
    private readonly AppDbContext _dbContext;

    public EvenementQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CommentListReadModel> GetCommentsAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<EventDetailsReadModel?> GetEventByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _dbContext.Evenements
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new EventDetailsReadModel
            {
                Id = e.Id,
                Titre = e.Titre,
                Description = e.Description,
                PersonneImpliquee = e.PersonneImpliquee,
                Statut = e.Statut,
                DateCreationUtc = e.DateCreationUtc,
                DateMiseAjourUtc = e.DateMiseAjourUtc
            })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<EventListItemReadModel>> GetEventsPagedAsync(
        GetEventsPagedQuery query,
        CancellationToken ct = default)
    {
        var baseQuery = _dbContext.Evenements
            .AsNoTracking();

        // Filtre par statut
        if (!string.IsNullOrWhiteSpace(query.Statut))
        {
            baseQuery = baseQuery.Where(
                e => e.Statut.Code == query.Statut);
        }

        // Nombre total avant pagination
        var totalCount = await baseQuery.CountAsync(ct);

        // Pagination
        var items = await baseQuery
            .OrderByDescending(e => e.DateCreationUtc)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(e => new EventListItemReadModel
            {
                Id = e.Id,
                Titre = e.Titre,
                PersonneImpliquee = e.PersonneImpliquee
            })
            .ToListAsync(ct);

        return new PagedResult<EventListItemReadModel>(
            items,
            totalCount,
            query.Page,
            query.PageSize);
    }

   
}