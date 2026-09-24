// src/EventManagement.Infrastructure/QueryServices/CommentQueryService.cs

namespace EventManagement.Infrastructure.QueryServices;

using EventManagement.Application.Interfaces;
using EventManagement.Application.ReadModels;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

/*Distinction Command / Query



Aspect                              Command (écriture)                                                       Query (lecture)



Accès               Via Event uniquement (event.AddComment(),      Direct sur la table comments
                       event.UpdateComment()) 
DbSet nécessaire    Non                                            Oui (ou Set<Comment>())
Tracking            Oui (changements via l’agrégat)                Non (AsNoTracking)
Objectif            Protéger les invariants métier                 Performance et projection
 */
public sealed class CommentaireQueryService : ICommentaireQueryService
{
    private readonly AppDbContext _dbContext;

    public CommentaireQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommentReadModel>> GetCommentsByEventIdAsync(
        Guid eventId,
        CancellationToken ct)
    {
        return await _dbContext.Commentaires
            .AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.DateCreationUtc)
            .Select(c => new CommentReadModel(
                c.Id,
                c.EventId,
                c.Description,
                c.DateCreationUtc,
                c.DateMiseAJourUtc))
            .ToListAsync(ct);
    }

    public async Task<CommentReadModel?> GetByIdAsync(
        Guid commentId,
        CancellationToken ct)
    {
        return await _dbContext.Commentaires
            .AsNoTracking()
            .Where(c => c.Id == commentId)
            .Select(c => new CommentReadModel(
                c.Id,
                c.EventId,
                c.Description,
                c.DateCreationUtc,
                c.DateMiseAJourUtc))
            .FirstOrDefaultAsync(ct);
    }
}
/* Si on ne veut pas utiliser le DbSet<Commentaire> dans le DbContext, on peut utiliser la méthode Set<Commentaire>() pour accéder à l'entité Commentaire. Cela permet de garder le DbContext propre et de ne pas exposer directement les entités internes. */
/* Note : Le code ci-dessus suppose que l'entité Commentaire a les propriétés Id, EventId, Content, CreatedAtUtc et UpdatedAtUtc. Assurez-vous que ces propriétés existent dans votre entité Commentaire. */
/*public async Task<IReadOnlyList<CommentReadModel>> GetCommentsByEventIdAsync(
    Guid eventId,
    CancellationToken ct)
{
    return await _dbContext.Set<Comment>()  // <-- Pas de DbSet déclaré
        .AsNoTracking()
        .Where(c => c.EventId == eventId)
        .OrderByDescending(c => c.CreatedAtUtc)
        .Select(c => new CommentReadModel(...))
        .ToListAsync(ct);
}
 */