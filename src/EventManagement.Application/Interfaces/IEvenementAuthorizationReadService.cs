using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Application.Interfaces
{
    //Note : pas de Statut dans ces records. Le statut est une préoccupation du domaine, pas de l'autorisation.
    public sealed record EvenementAuthorizationInfo(Guid Id, Guid ProprietaireId);

    public interface IEvenementAuthorizationReadService
    {
        Task<EvenementAuthorizationInfo?> GetForAuthorizationAsync(Guid id, CancellationToken ct);
    }
}
