using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//namespace EventManagement.Api.Tests

namespace EventManagement.Api.Tests.Integration.Helpers;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;



/// <summary>
/// Générateur de JWT pour les tests d'intégration.
/// Utilise la même clé symétrique, issuer et audience que l'API en runtime
/// afin que AddJwtBearer valide correctement le token sans mock.
/// </summary>
public static class JwtTestTokenBuilder
{
    // Clé de test minimale (>= 32 caractères pour HMAC-SHA256).
    // Doit correspondre exactement à la clé injectée dans l'API durant les tests.
    private const string TestSigningKey = "votre-cle-super-secrete-de-32-char!";

    private static readonly SymmetricSecurityKey SecurityKey = new(
        Encoding.UTF8.GetBytes(TestSigningKey));

    private static readonly SigningCredentials Credentials = new(
        SecurityKey, SecurityAlgorithms.HmacSha256);

    /// <summary>
    /// Crée un JWT signé compatible avec la validation de l'API.
    /// </summary>
    /// <param name="userId">Identité de l'utilisateur (sub + nameidentifier).</param>
    /// <param name="role">Rôle unique (Administrator, Organizer, Participant).</param>
    /// <param name="permissions">Permissions émises dans le token (ex: Events.Read).</param>
    /// <param name="expiresIn">Durée de validité du token (défaut 1 heure).</param>
    public static string CreateToken(
        Guid userId,
        string role,
        string[] permissions,
        TimeSpan? expiresIn = null)
    {
        var claims = new List<Claim>
        {
            // sub : standard JWT, lu par GetUserId en fallback
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),

            // nameidentifier : lu en priorité par GetUserId (ClaimTypes.NameIdentifier)
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),

            // role : utilisé par IsInRole, GetRoles, IsAdministrator
            new Claim(ClaimTypes.Role, role)
        };

        // Permissions : un claim par permission pour compatibilité avec la policy
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromHours(1)),
            Issuer = "EventManagementLab",
            Audience = "EventManagementLab",
            SigningCredentials = Credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(descriptor);
        return handler.WriteToken(token);
    }

    /// <summary>
    /// Expose la clé de test pour que le WebApplicationFactory configure
    /// l'API avec la même valeur (évite la dépendance aux user-secrets en test).
    /// </summary>
    public static string SigningKey => TestSigningKey;
}

