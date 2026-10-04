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
    public sealed class EvenementAuthorizationReadService : IEvenementAuthorizationReadService
    {
        private readonly AppDbContext _db;
        public EvenementAuthorizationReadService(AppDbContext db) => _db = db;

        public async Task<EvenementAuthorizationInfo?> GetForAuthorizationAsync(Guid id, CancellationToken ct)
        {
            return await _db.Evenements
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new EvenementAuthorizationInfo(e.Id, e.ProprietaireId))
                .FirstOrDefaultAsync(ct);
        }
    }
}
