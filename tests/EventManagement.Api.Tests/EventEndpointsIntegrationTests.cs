using EventManagement.Api.Tests.Integration.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EventManagement.Api.Tests.Integration.Endpoints;

public class EventEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    //(401/403/404/409/200-204)
    /*
     Scénario                                                    Attendu
     PUT /events/{id} sans JWT                                   401
     PUT /events/{id} JWT sans permission Events.Update	        403
     PUT /events/{id} permission OK, event inexistant	        404
     PUT /events/{id} permission OK, event d'un autre (non Admin)403
     PUT /events/{id} propriétaire, event Annule	                409
     PUT /events/{id} propriétaire, event Draft	                204
     PUT /events/{id} Administrator, event d'un autre	        204
     GET /events/{id} permission Events.Read                     200
     POST /events permission Events.Create	201
     POST /events rôle Participant (sans Events.Create)	        403
     PUT /events/{eid}/ comments /{ cid} JWT, non auteur (non Admin)	403
     PUT /events/{eid}/ comments /{ cid} auteur                  204
     Token admin sans claim permission	                        403 (piège §1.3)
     GET /me sans JWT	                                        401
     GET /me JWT valide (rôle Organizer)	                        200 + { userId, roles: ["Organizer"] }
     GET /me JWT signé sans claim sub	                        401 (et non 500 — cf. §10)

     Critères d'acceptation
  Tout endpoint est protégé par défaut (RequireAuthorization() sur le groupe).
  Aucun AuthorizationHandler ne référence le Statut.
  Aucun User.IsInRole(...) en dehors de Api/Security.
  L'identité circule par valeur dans les commands, jamais via IHttpContextAccessor.
  Commentaire.AuteurId exploité avec un handler dédié.
  Clé JWT hors du code source (config + User Secrets).
  RoleClaimType = ClaimTypes.Role configuré explicitement.
  Codes HTTP strictement sémantiques (401/403/404/409/400/200/201/204).
  Les filtres de ressource font une projection légère (Id, ProprietaireId/AuteurId), pas un chargement d'agrégat.
  Chaque filtre lit sa clé de route (id pour les événements, commentaireId pour les commentaires).
  Une seule policy de ressource par agrégat (Event.Manage, Comment.Manage).
  Double lecture (filtre léger → handler complet) documentée comme concession du Labo 2.
  Les rôles vivent dans Api/Security, jamais dans Application ni Domain.
  Aucun rôle n'est stocké en base ; le mapping rôle → permissions est code-defined.
  Le jeton d'un Administrator porte toutes les permissions (cf. §1.3).
  Aucun endpoint d'émission de jetons n'est actif hors Development.
  GetRoles() n'est utilisé que pour informer, jamais pour décider.
  (v3.1) L'endpoint /me vit dans Api/Endpoints/UserEndpoints.cs, jamais dans EventEndpoints.cs ni CommentEndpoints.cs.
  (v3.1) /me n'a ni filtre de ressource, ni policy de permission, ni handler CQRS.
  (v3.1) GetUserId() lève MissingUserClaimException, jamais UnauthorizedAccessException.
  (v3.1) MissingUserClaimException est mappée en 401 ; aucun chemin ne produit un 500 pour un défaut d'identité.
  (v3.1) app.UseAuthentication() puis app.UseAuthorization() précèdent le mapping des endpoints.
  (v3.1) Le package Microsoft.AspNetCore.Authentication.JwtBearer est référencé par EventManagement.Api
    */

    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public EventEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remplacer la connexion PostgreSQL par une base en mémoire
                // ou utiliser Testcontainers pour PostgreSQL selon le niveau du labo
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetEvents_WithoutAuth_Returns401()
    {
        // Pas de header Authorization
        var response = await _client.GetAsync("/api/events");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEvent_WithoutPermission_Returns403()
    {
        var token = JwtTestTokenBuilder.CreateToken(
            userId: Guid.NewGuid(),
            role: "Participant",      // pas la permission Events.Update
            permissions: new[] { "Events.Read" });

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PutAsJsonAsync(
            "/api/events/123e4567-e89b-12d3-a456-426614174000",
            new { Titre = "Nouveau titre" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_UnknownId_Returns404()
    {
        var token = JwtTestTokenBuilder.CreateToken(
            userId: Guid.NewGuid(),
            role: "Participant",
            permissions: new[] { "Events.Read" });

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync(
            "/api/events/123e4567-e89b-12d3-a456-426614174999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CancelEvent_AlreadyCanceled_Returns409()
    {
        // Setup : créer un événement annulé en base, puis tenter de l'annuler à nouveau
    }

    [Fact]
    public async Task CreateEvent_WithValidPermission_Returns201()
    {
        var token = JwtTestTokenBuilder.CreateToken(
            userId: Guid.NewGuid(),
            role: "Organizer",
            permissions: new[] { "Events.Create" });

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            Titre = "Nouvelle conférence",
            DateDebut = DateTime.UtcNow.AddDays(1),
            Lieu = "Montréal"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}