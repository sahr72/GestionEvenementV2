using EventManagement.Api.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventManagement.Api.Endpoints
{
    public static class UserEndpoints
    {
        /*
         * Pourquoi ce fichier et pas un autre :

            Emplacement	        Verdict	     Raison
            EventEndpoints.cs	❌	         Le fichier sert la ressource Événement ; /me n'y touche pas
            CommentEndpoints.cs	❌	         Idem, pour la ressource Commentaire
            UserEndpoints.cs	✅	         Sert l'identité de l'appelant : c'est une ressource à part entière
            Pourquoi le groupe /api/me et non /api/users/me : la variante imbriquée entrerait en collision visuelle avec une future 
                    route /api/users/{id:guid}. /api/me est une convention reconnue et évite la question.

            Pourquoi MeResponse est un record nommé : Swagger génère alors un schéma exploitable, là où un objet anonyme produit un schéma vide.

            Ce qu'il ne faut PAS y mettre :

            À éviter	                                        Raison
            AddEndpointFilter<...ResourceAuthorizationFilter>()	Aucune ressource à charger : pas de projection, pas de 404 possible
            Un GetMeQueryHandler CQRS	                        La donnée vient du jeton, pas de la base : aucune requête à émettre
            Un http.User.IsInRole(...)	                        Règle du labo : aucune décision de rôle hors Api/Security
            Une policy de permission	                        .RequireAuthorization() sans policy = « authentifié suffit », exactement le besoin

            Précision importante : GetRoles() expose une donnée, il ne décide rien. Le critère « aucun IsInRole hors Api/Security » 
                    vise IsInRole en tant que décision : il reste respecté. C'est la distinction « informer vs autoriser ».

            Emplacement de /dev/token (si implémenté) : ce même fichier, sous if (app.Environment.IsDevelopment()). C'est aussi de l'identité.
         * */
        //public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        //{
        //    var group = app.MapGroup("/api/me")
        //        .WithTags("Me")
        //        .RequireAuthorization();   // deny by default : authentifié suffit

        //    // "" (et non "/") pour viser exactement /api/me, sans slash terminal ambigu.
        //    group.MapGet("", (HttpContext http) => Results.Ok(new MeResponse(
        //        UserId: http.User.GetUserId(),
        //        Roles: http.User.GetRoles())));

        //    return app;
        //}
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/me")
                .WithTags("Me")
                .RequireAuthorization();

            group.MapGet("", (HttpContext http) => Results.Ok(new MeResponse(
                UserId: http.User.GetUserId(),
                Roles: http.User.GetRoles())));

            // Endpoint DEV uniquement pour générer un token de test
            // Pas de .RequireAuthorization() ici, évidemment
            app.MapPost("/dev/token", (
                GenerateTokenRequest request,
                IConfiguration config,
                IHostEnvironment env) =>
            {
                if (!env.IsDevelopment())
                    return Results.Problem("Disponible uniquement en développement.", statusCode: 403);

                var signingKey = config["Jwt:SigningKey"];
                if (string.IsNullOrEmpty(signingKey))
                    return Results.Problem("Jwt:SigningKey non configurée.", statusCode: 500);

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

                foreach (var role in request.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                    // Permissions basées sur les rôles pour le labo
                    if (role == "Administrator")
                    {
                        claims.Add(new Claim("permission", "Event.Manage.All"));
                        claims.Add(new Claim("permission", "Comment.Manage.All"));
                    }
                    if (role == "User")
                    {
                        claims.Add(new Claim("permission", "Event.Create"));
                        claims.Add(new Claim("permission", "Event.Read"));
                        claims.Add(new Claim("permission", "Comment.Create"));
                        claims.Add(new Claim("permission", "Comment.Read"));
                    }
                }

                var token = new JwtSecurityToken(
                    issuer: config["Jwt:Issuer"] ?? "EventManagementLab",
                    audience: config["Jwt:Audience"] ?? "EventManagementLab",
                    claims: claims,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: credentials);

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Results.Ok(new { Token = tokenString });
            })
            .WithTags("Dev")
            .AllowAnonymous();

            return app;
        }

        public sealed record GenerateTokenRequest(Guid UserId, List<string> Roles);
    }

    public sealed record MeResponse(Guid UserId, IReadOnlyList<string> Roles);
}
