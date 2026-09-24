using EventManagement.Domain.Entities;

namespace EventManagement.Application.Interfaces;
/*
 * Spécialisé par Aggregate ( Evenement), pas générique.
   GetByIdAsyncpeut être utilisé pour charger un agrégat en mode modification.
   SaveChangesAsyncest exposé (pas de wrapper UnitOfWork artificiel).
 */
public interface IEvenementRepository
{
    Task<Evenement?> GetByIdWithCommentsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Evenement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Evenement evenement, CancellationToken cancellationToken = default);
    void Update(Evenement evenement);
    void Delete(Evenement evenement);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}