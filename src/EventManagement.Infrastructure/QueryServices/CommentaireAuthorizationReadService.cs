using EventManagement.Application.Interfaces;
using EventManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Infrastructure.QueryServices
{
    public sealed class CommentaireAuthorizationReadService : ICommentaireAuthorizationReadService
    {
        private readonly AppDbContext _db;
        public CommentaireAuthorizationReadService(AppDbContext db) => _db = db;

        public async Task<CommentaireAuthorizationInfo?> GetForAuthorizationAsync(Guid id, CancellationToken ct)
        {
            return await _db.Commentaires
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CommentaireAuthorizationInfo(c.Id, c.EventId, c.AuteurId))
                .FirstOrDefaultAsync(ct);
        }
    }
}
