using EventManagement.Api.Security.TokenIssuance;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace EventManagement.Api.Security
{
    public static class ClaimsPrincipalExtensions
    {
        /*
         *  la v3 levait UnauthorizedAccessException, exception BCL générique également émise par le runtime pour des accès fichiers/OS refusés. 
         *  La mapper en 401 aurait donc masqué des erreurs réelles derrière un code d'authentification trompeur. On introduit un type dédié. 
         * */
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? principal.FindFirstValue("sub");

            if (!Guid.TryParse(value, out var id))
               // throw new UnauthorizedAccessException("Claim identifiant utilisateur absente ou invalide.");
                throw new MissingUserClaimException(ClaimTypes.NameIdentifier);   // → 401 (voir §10)

            return id;
        }

        // Ajout v3 : lecture du/des rôle(s). Confinée à Api/Security.
        public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal principal)
            => principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        public static bool IsAdministrator(this ClaimsPrincipal principal)
            => principal.IsInRole(Roles.Administrator);
    }
}
