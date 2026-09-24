// src/EventManagement.Infrastructure/Repositories/EventRepository.cs

namespace EventManagement.Infrastructure.Repositories;
using EventManagement.Application.Interfaces;

using EventManagement.Domain.Entities;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public sealed class EvenementRepository : IEvenementRepository
{
    private readonly AppDbContext _dbContext;

    public EvenementRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Evenement?> GetByIdWithCommentsAsync(Guid id, CancellationToken ct)
    {
        return await _dbContext.Evenements
            .Include(e => e.Commentaires)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public void Update(Evenement aggregate)
    {
        _dbContext.Evenements.Update(aggregate);
    }

    public async Task AddAsync(Evenement aggregate, CancellationToken ct)
    {
        await _dbContext.Evenements.AddAsync(aggregate, ct);
    }

    public void Delete(Evenement aggregate)
    {
        _dbContext.Evenements.Remove(aggregate);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _dbContext.SaveChangesAsync(ct);
    }

    public Task<Evenement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Evenements.FindAsync(new object[] { id }, cancellationToken).AsTask();
    }
}