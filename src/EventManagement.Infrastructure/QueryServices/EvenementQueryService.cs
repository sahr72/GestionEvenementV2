// src/EventManagement.Infrastructure/QueryServices/EventQueryService.cs

namespace EventManagement.Infrastructure.QueryServices;

using EventManagement.Application.Interfaces;
using EventManagement.Application.ReadModels;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public sealed class EvenementQueryService : IEvenementQueryService
{
    private readonly AppDbContext _dbContext;

    public EvenementQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CommentListReadModel> GetCommentsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<EventDetailsReadModel?> GetEventByIdAsync(Guid id, CancellationToken ct)
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
                Statut = e.Statut,//statut.code
                DateCreationUtc = e.DateCreationUtc,
                DateMiseAjourUtc = e.DateMiseAjourUtc
            })
            .FirstOrDefaultAsync(ct);
    }

    public Task<EventListReadModel> GetEventsPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}