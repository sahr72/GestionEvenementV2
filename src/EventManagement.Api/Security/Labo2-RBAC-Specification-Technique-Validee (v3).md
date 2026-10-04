# Labo 2 — RBAC & Resource Authorization — Spécification technique validée

**Projet :** EventManagement V2 — Labo niveau Moyen (CQRS pragmatique)
**Framework :** .NET 8 (figé)
**Statut :** Spécification révisée (v3.1) — emplacement du `/me` et traduction de l'erreur d'identité tranchés
**Révision :** v3.1 complète la v3 sur trois points de contrat (arborescence des endpoints, `MissingUserClaimException`, mapping 401)

---

## 0. Principe directeur

Une règle, un seul endroit d'autorité :

```text
JWT             → Qui es-tu ?
Permission      → Qu'as-tu le droit de tenter ?
Resource        → Sur CETTE ressource ?
Application     → Quel cas d'usage ?
Domain          → Est-ce légal dans l'état actuel ?
EF Core         → Comment persister ?
```

Règles inviolables :

- `Events.Update ≠ "peut modifier CET événement"` (permission globale ≠ autorisation ressource).
- Le domaine ne connaît **jamais** les rôles, claims ou `HttpContext`.
- L'identité traverse la frontière **en tant que valeur** (`Guid userId`), jamais comme contexte HTTP.
- `Deny by default` : tout endpoint est protégé sauf décision explicite contraire.

---

## 1. Décisions figées

| Sujet | Décision |
|---|---|
| Framework | **.NET 8** |
| Mécanisme ASP.NET Authorization | Dans **`Api/Security`** (extension, requirements, handlers, filtres) |
| `Application` | Ne porte que le **vocabulaire** : `Permissions`, `Policies`, `ResourceOperations` + les commands (`Guid userId`) |
| Permission vs Ressource | Deux familles de policies distinctes, jamais mélangées |
| Policies de ressource | **Une par agrégat** (`Event.Manage`, `Comment.Manage`) — voir §1.1 |
| Rôles | **3 rôles de référence** (`Administrator`, `Organizer`, `Participant`) ; vivent dans `Api/Security`, façonnent le **contenu du jeton**. Voir §1.2 |
| Rôle lisible par le code | Seul `Administrator` est lu en code (`IsInRole`, bypass **ressource**). Voir §1.3 |
| `RoleClaimType` | `ClaimTypes.Role` explicitement configuré |
| Commentaires | `Commentaire.AuteurId` + handler dédié (`Auteur || Admin`) |
| Suppression d'événement | Abandonnée → `Cancel` (DDD), pas de `Delete` métier |
| Resource check | **Read service léger** (`Id, ProprietaireId` seulement) + **Endpoint Filter spécifique** |
| **Endpoint `/me`** | **`Api/Endpoints/UserEndpoints.cs`**, groupe **`/api/me`**, `RequireAuthorization()` seule (voir §7.5) |
| **Claim d'identité absent** | `MissingUserClaimException` (dans `Api/Security`) mappée en **401** (voir §10) |
| TOCTOU | Concession assumée du Labo 2 (concurrence optimiste = Labo 3) |
| Clé JWT | Config + User Secrets, `DateTime.UtcNow` |
| Statuts HTTP | 401 / 403 / 404 / 409 strictement sémantiques |

### 1.1 Correction v2 — une seule policy de ressource par agrégat

**Défaut de la v1 :** elle déclarait quatre policies de ressource (`Event.Update.Resource`, `Event.Confirm.Resource`, `Event.Cancel.Resource`, `Event.Close.Resource`) pointant vers **le même** `EvenementResourceRequirement` (règle identique `Proprietaire || Admin`). Le filtre, lui, autorisait en dur contre `Event.Update.Resource` — y compris pour `/confirm`, `/cancel`, `/close`. Redondant et **trompeur**.

**Décision v2 :** la règle `Proprietaire || Admin` ne dépend **pas** de l'opération. On déclare donc **une** policy de ressource par agrégat :

```text
Event.Manage      → Proprietaire || Admin     (update, confirm, cancel, close)
Comment.Manage    → Auteur || Admin           (update, delete)
```

Si un jour une opération doit diverger (ex. : seul le propriétaire peut clôturer), on **scinde** à ce moment-là. On ne crée pas quatre policies identiques « au cas où » : c'est de l'overengineering.

### 1.2 Ajout v3 — les rôles ne sont pas une couche d'autorisation

La décision structurante du Labo 2 reste : **l'autorité d'endpoint, c'est la permission** ; **l'autorité sur une ressource, c'est `Owner/Auteur`**. Le rôle n'ajoute **aucun chemin de code** nouveau. Il n'intervient qu'à **l'émission du jeton**, pour décider *quelles permissions* le jeton transporte — plus, pour `Administrator` uniquement, le bypass ressource déjà existant.

```text
Rôle  ─(façonne)→  contenu du jeton (claims role + permission)
                   │
                   ├─ permissions  → jouées à l'étape « permission »
                   └─ claim role   → jouée à l'étape « ressource » (Administrator seulement)
```

Conséquence assumée : `Organizer` et `Participant` **n'existent pas dans le code de l'API**. Ce sont des *bundles de permissions*. On ne crée donc ni `Role` entity, ni `RoleRepository`, ni table de rôles (→ Labo 3 si besoin réel).

### 1.3 Correction v3 — `Administrator` a **aussi** besoin des permissions

Piège corrigé par rapport au raisonnement v2 : `IsInRole("Administrator")` n'agit **qu'à l'étape ressource**. À l'étape permission, la policy `PermissionRequirement` exige toujours la claim `permission=<X>`. Un jeton administrateur **doit** donc porter **toutes les permissions** ; sinon l'admin reçoit **403 à l'étape permission**, avant même que son bypass ressource ne puisse jouer.

```text
Administrator = "toutes les permissions"  +  "bypass ressource (IsInRole)"
                 └── conditionné à l'étape permission ──┘
```

### 1.4 Ajout v3.1 — `/me` n'est pas une ressource

Le endpoint `/me` sert **l'identité de l'appelant**, pas une ressource métier. Il n'a donc ni projection légère, ni filtre de ressource, ni 404 possible. Sa donnée vient du **jeton**, pas de la base : aucune requête n'est émise, aucun handler CQRS n'est requis.

Corollaire de rangement : il ne vit **ni** dans `EventEndpoints.cs` **ni** dans `CommentEndpoints.cs`, qui sont organisés **par ressource exposée**. Voir §7.5.

---

## 2. Arborescence cible

```text
EventManagement
│
├── src/EventManagement.Domain
│   └── Entities
│       ├── Evenement.cs        (Aggregate Root)
│       └── Commentaire.cs      (entité enfant)
│
├── src/EventManagement.Application
│   ├── Authorization
│   │   ├── Permissions.cs
│   │   ├── Policies.cs
│   │   └── ResourceOperations.cs
│   └── (Features existantes : Commands, Queries, ReadModels, Interfaces, Validators...)
│
├── src/EventManagement.Infrastructure
│   ├── Persistence
│   │   └── AppDbContext.cs
│   ├── Repositories
│   │   └── EvenementRepository.cs
│   └── QueryServices
│       ├── EvenementAuthorizationReadService.cs
│       └── CommentaireAuthorizationReadService.cs
│
└── src/EventManagement.Api
    ├── Endpoints                        (v3.1 — organisation PAR RESSOURCE)
    │   ├── EventEndpoints.cs            (ressource Evenement)
    │   ├── CommentEndpoints.cs          (ressource Commentaire)
    │   └── UserEndpoints.cs             (identité — /api/me, /dev/token)   ← ajout v3.1
    ├── Middleware
    │   └── GlobalExceptionHandler.cs    (seul point de traduction vers HTTP)
    └── Security
        ├── ClaimsPrincipalExtensions.cs
        ├── MissingUserClaimException.cs                     ← ajout v3.1
        ├── TokenIssuance
        │   ├── Roles.cs
        │   ├── RolePermissions.cs
        │   └── JwtTokenFactory.cs
        ├── Requirements
        │   ├── PermissionRequirement.cs
        │   ├── EvenementResourceRequirement.cs
        │   └── CommentaireResourceRequirement.cs
        ├── Handlers
        │   ├── PermissionAuthorizationHandler.cs
        │   ├── EvenementResourceAuthorizationHandler.cs
        │   └── CommentaireResourceAuthorizationHandler.cs
        └── Filters
            ├── EvenementResourceAuthorizationFilter.cs
            └── CommentaireResourceAuthorizationFilter.cs
```

> **Correction v3.1 :** la v3 présentait une arborescence ne montrant que `Security`. Or la spécification référençait déjà `GlobalExceptionHandler` (§10) et le projet réel contient `Endpoints/` et `Middleware/`. Une arborescence dite « cible » qui omet des dossiers existants et cités ailleurs est un défaut de cohérence : elle est désormais complète, et la **règle d'organisation des endpoints** y est rendue explicite.

> **Règle d'organisation des endpoints (v3.1) :** **un fichier par ressource exposée**. `Evenement` → `EventEndpoints.cs` ; `Commentaire` → `CommentEndpoints.cs` ; **identité** → `UserEndpoints.cs`. Un endpoint qui ne sert aucune ressource métier ne se range pas « faute de mieux » dans un fichier de ressource : il obtient le sien.

> Remarque : le domaine utilise le vocabulaire métier français (`Evenement`, `Commentaire`, `EvenementStatut`) ; les **permissions/policies/rôles** gardent la notation anglaise technique (`Events.Update`, `Administrator`).

---

## 3. Domaine — ajustements requis

### 3.1 `Evenement.cs`

```csharp
public Guid ProprietaireId { get; private set; }   // OwnerId

public static Evenement Create(
    string titre, string description, string personneImpliquee,
    Guid proprietaireId, DateTimeOffset now)
{
    // ... validations existantes ...
    return new Evenement
    {
        Id = Guid.NewGuid(),
        Titre = titre.Trim(),
        Description = description.Trim(),
        PersonneImpliquee = personneImpliquee.Trim(),
        ProprietaireId = proprietaireId,
        Statut = EvenementStatut.Draft,
        CreatedAtUtc = now
    };
}

// Garde d'état métier — PAS une règle de rôle.
public bool PeutEtreModifie()
    => Statut is not (EvenementStatut.Annule or EvenementStatut.Cloture);

public void Modifier(string titre, string description, string personne, DateTimeOffset now)
{
    if (!PeutEtreModifie())
        throw new DomainException("L'événement ne peut plus être modifié dans son état actuel.");
    // ... assignations ...
    UpdatedAtUtc = now;
}

public Commentaire AjouterCommentaire(string contenu, Guid auteurId, DateTimeOffset now)
{
    EnsureCommentable();
    var commentaire = new Commentaire(Guid.NewGuid(), Id, auteurId, contenu, now);
    _commentaires.Add(commentaire);
    return commentaire;
}
```

### 3.2 `Commentaire.cs`

```csharp
public Guid AuteurId { get; private set; }   // AuthorId

internal Commentaire(Guid id, Guid evenementId, Guid auteurId, string contenu, DateTimeOffset now)
{
    if (string.IsNullOrWhiteSpace(contenu))
        throw new DomainException("Le contenu du commentaire est obligatoire.");
    Id = id;
    EvenementId = evenementId;
    AuteurId = auteurId;
    Contenu = contenu.Trim();
    CreatedAtUtc = now;
}
```

### 3.3 Configurations EF Core

```csharp
// EvenementConfiguration
builder.Property(e => e.ProprietaireId).IsRequired();

// CommentaireConfiguration
builder.Property(c => c.AuteurId).IsRequired();
```

### 3.4 Migration

```powershell
Add-Migration AddOwnership -Project EventManagement.Infrastructure -StartupProject EventManagement.Api
Update-Database -Project EventManagement.Infrastructure -StartupProject EventManagement.Api
```

> **Note v3 :** aucun changement de schéma n'est apporté par les rôles. Les rôles vivent **dans le jeton**, pas en base. La migration `AddOwnership` reste la seule de ce labo.

---

## 4. Application — vocabulaire d'autorisation

### 4.1 `Permissions.cs`

```csharp
namespace EventManagement.Application.Authorization;

public static class Permissions
{
    public static class Events
    {
        public const string Read = "Events.Read";
        public const string Create = "Events.Create";
        public const string Update = "Events.Update";
        public const string Confirm = "Events.Confirm";
        public const string Cancel = "Events.Cancel";
        public const string Close = "Events.Close";
    }

    public static class Comments
    {
        public const string Read = "Comments.Read";
        public const string Create = "Comments.Create";
        public const string Update = "Comments.Update";
        public const string Delete = "Comments.Delete";
    }
}
```

### 4.2 `Policies.cs` — endpoint-level (valeurs **dérivées**, aucune divergence possible)

```csharp
namespace EventManagement.Application.Authorization;

public static class Policies
{
    public static class Events
    {
        public const string Read = Permissions.Events.Read;
        public const string Create = Permissions.Events.Create;
        public const string Update = Permissions.Events.Update;
        public const string Confirm = Permissions.Events.Confirm;
        public const string Cancel = Permissions.Events.Cancel;
        public const string Close = Permissions.Events.Close;
    }

    public static class Comments
    {
        public const string Read = Permissions.Comments.Read;
        public const string Create = Permissions.Comments.Create;
        public const string Update = Permissions.Comments.Update;
        public const string Delete = Permissions.Comments.Delete;
    }
}
```

### 4.3 `ResourceOperations.cs` — noms des policies **sur ressource** (usage exclusif avec `AuthorizeAsync`)

```csharp
namespace EventManagement.Application.Authorization;

public static class ResourceOperations
{
    // Une policy par agrégat : la règle Proprietaire||Admin est identique
    // pour update/confirm/cancel/close. On ne duplique pas.
    public static class Events
    {
        public const string Manage = "Event.Manage.Resource";
    }

    // La règle Auteur||Admin est identique pour update/delete.
    public static class Comments
    {
        public const string Manage = "Comment.Manage.Resource";
    }
}
```

### 4.4 Contrat de lecture légère (côté lecture CQRS)

```csharp
namespace EventManagement.Application.Interfaces;

public sealed record EvenementAuthorizationInfo(Guid Id, Guid ProprietaireId);

public interface IEvenementAuthorizationReadService
{
    Task<EvenementAuthorizationInfo?> GetForAuthorizationAsync(Guid id, CancellationToken ct);
}
```

```csharp
namespace EventManagement.Application.Interfaces;

public sealed record CommentaireAuthorizationInfo(Guid Id, Guid EvenementId, Guid AuteurId);

public interface ICommentaireAuthorizationReadService
{
    Task<CommentaireAuthorizationInfo?> GetForAuthorizationAsync(Guid id, CancellationToken ct);
}
```

> **Note** : pas de `Statut` dans ces records. Le statut est une préoccupation du **domaine**, pas de l'autorisation.

### 4.5 Les commands portent l'identité

```csharp
public sealed record CreateEventCommand(string Titre, string Description, string PersonneImpliquee, Guid ProprietaireId);
public sealed record AddCommentCommand(Guid EvenementId, string Contenu, Guid AuteurId);
```

Aucun handler n'accède à `HttpContext` ou `ClaimsPrincipal`.

> **Note v3 :** les rôles **n'apparaissent pas ici**. L'Application ne connaît ni `Administrator`, ni `Organizer`, ni `Participant`. Elle ne reçoit qu'un `Guid userId` et des permissions déjà résolues en policies.

---

## 5. Infrastructure — read services légers

```csharp
namespace EventManagement.Infrastructure.QueryServices;

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
```

```csharp
namespace EventManagement.Infrastructure.QueryServices;

public sealed class CommentaireAuthorizationReadService : ICommentaireAuthorizationReadService
{
    private readonly AppDbContext _db;
    public CommentaireAuthorizationReadService(AppDbContext db) => _db = db;

    public async Task<CommentaireAuthorizationInfo?> GetForAuthorizationAsync(Guid id, CancellationToken ct)
    {
        return await _db.Commentaires
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CommentaireAuthorizationInfo(c.Id, c.EvenementId, c.AuteurId))
            .FirstOrDefaultAsync(ct);
    }
}
```

> **Correction v3.1 :** le namespace est `EventManagement.Infrastructure.QueryServices` (et non `.Queries`), aligné sur le dossier réel et sur les `using` du `Program.cs`.

---

## 6. Api/Security — mécanisme ASP.NET Core

### 6.1 `ClaimsPrincipalExtensions.cs` (étendu v3, durci v3.1)

```csharp
namespace EventManagement.Api.Security;

using System.IdentityModel.Tokens.Jwt;   // JwtRegisteredClaimNames
using System.Security.Claims;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        // NameIdentifier en premier (NameClaimType configuré, §6.6), "sub" en repli.
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(value, out var id))
            throw new MissingUserClaimException(ClaimTypes.NameIdentifier);   // → 401 (voir §10)

        return id;
    }

    // Ajout v3 : lecture du/des rôle(s). Confinée à Api/Security.
    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal principal)
        => principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

    public static bool IsAdministrator(this ClaimsPrincipal principal)
        => principal.IsInRole(Roles.Administrator);
}
```

> **Correction v3.1 :** la v3 levait `UnauthorizedAccessException`, exception **BCL générique** également émise par le runtime pour des accès fichiers/OS refusés. La mapper en 401 aurait donc masqué des erreurs réelles derrière un code d'authentification trompeur. On introduit un type **dédié** (§6.2).

### 6.2 `MissingUserClaimException.cs` (ajout v3.1)

```csharp
namespace EventManagement.Api.Security;

// Exception dédiée : le jeton est authentifiable mais ne porte pas d'identité exploitable.
// Elle vit dans Api/Security : c'est une préoccupation d'IDENTITÉ, pas de domaine ni d'application.
public sealed class MissingUserClaimException(string claimType)
    : Exception($"La claim d'identité '{claimType}' est absente ou invalide dans le jeton.")
{
    public string ClaimType { get; } = claimType;
}
```

> **Pourquoi dans `Api/Security` et non dans `Application` ou `Domain` ?** L'Application ne connaît ni les claims ni les jetons (§4.5) ; le Domaine encore moins (§0). Placer ce type ailleurs ferait fuiter une notion d'identité HTTP dans les couches basses — exactement ce que le labo interdit. `Api/Security` et `Api/Middleware` appartiennent au **même assembly** : le `GlobalExceptionHandler` peut donc la référencer sans créer de dépendance nouvelle.

### 6.3 Requirements

```csharp
// PermissionRequirement.cs
namespace EventManagement.Api.Security.Requirements;

using Microsoft.AspNetCore.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
```

```csharp
// EvenementResourceRequirement.cs
namespace EventManagement.Api.Security.Requirements;

using Microsoft.AspNetCore.Authorization;

public sealed class EvenementResourceRequirement : IAuthorizationRequirement { }
```

```csharp
// CommentaireResourceRequirement.cs
namespace EventManagement.Api.Security.Requirements;

using Microsoft.AspNetCore.Authorization;

public sealed class CommentaireResourceRequirement : IAuthorizationRequirement { }
```

### 6.4 Handlers

```csharp
// PermissionAuthorizationHandler.cs
namespace EventManagement.Api.Security.Handlers;

using EventManagement.Api.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
```

```csharp
// EvenementResourceAuthorizationHandler.cs
namespace EventManagement.Api.Security.Handlers;

using EventManagement.Api.Security;
using EventManagement.Api.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

public sealed class EvenementResourceAuthorizationHandler
    : AuthorizationHandler<EvenementResourceRequirement, EvenementAuthorizationInfo>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EvenementResourceRequirement requirement,
        EvenementAuthorizationInfo resource)
    {
        // Admin passe. Sinon, propriétaire uniquement.
        // AUCUNE référence au statut : c'est le domaine qui décide.
        if (context.User.IsAdministrator()             // constante, plus de chaîne magique (v3)
            || context.User.GetUserId() == resource.ProprietaireId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

```csharp
// CommentaireResourceAuthorizationHandler.cs
namespace EventManagement.Api.Security.Handlers;

using EventManagement.Api.Security;
using EventManagement.Api.Security.Requirements;
using Microsoft.AspNetCore.Authorization;

public sealed class CommentaireResourceAuthorizationHandler
    : AuthorizationHandler<CommentaireResourceRequirement, CommentaireAuthorizationInfo>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CommentaireResourceRequirement requirement,
        CommentaireAuthorizationInfo resource)
    {
        if (context.User.IsAdministrator()
            || context.User.GetUserId() == resource.AuteurId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### 6.5 Filtres d'endpoint — spécifiques et simples

> **Rappel d'ordre d'exécution (correction v2) :** les filtres s'exécutent **après** le middleware d'autorisation. `RequireAuthorization(...)` est évalué d'abord (→ 401/403 sur la *permission*), puis le filtre (→ 404/403 sur la *ressource*). Un filtre n'est donc atteint que par un utilisateur **déjà authentifié et porteur de la permission globale**.

```csharp
// EvenementResourceAuthorizationFilter.cs
namespace EventManagement.Api.Security.Filters;

using EventManagement.Api.Security;
using EventManagement.Application.Authorization;
using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

public sealed class EvenementResourceAuthorizationFilter(
    IEvenementAuthorizationReadService readService,
    IAuthorizationService authorization) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;

        // La contrainte de route {id:guid} garantit déjà un Guid valide.
        var id = (Guid)http.Request.RouteValues["id"]!;

        var info = await readService.GetForAuthorizationAsync(id, http.RequestAborted);
        if (info is null)
            return Results.NotFound();                       // 404 : ressource inexistante

        var result = await authorization.AuthorizeAsync(
            http.User, info, ResourceOperations.Events.Manage);  // policy unique par agrégat

        if (!result.Succeeded)
            return Results.Forbid();                         // 403 : ressource non accessible

        return await next(context);
    }
}
```

```csharp
// CommentaireResourceAuthorizationFilter.cs
namespace EventManagement.Api.Security.Filters;

using EventManagement.Api.Security;
using EventManagement.Application.Authorization;
using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

public sealed class CommentaireResourceAuthorizationFilter(
    ICommentaireAuthorizationReadService readService,
    IAuthorizationService authorization) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;

        // ATTENTION : la clé de route est "commentaireId", pas "id".
        var id = (Guid)http.Request.RouteValues["commentaireId"]!;

        var info = await readService.GetForAuthorizationAsync(id, http.RequestAborted);
        if (info is null)
            return Results.NotFound();                       // 404 : commentaire inexistant

        var result = await authorization.AuthorizeAsync(
            http.User, info, ResourceOperations.Comments.Manage);

        if (!result.Succeeded)
            return Results.Forbid();                         // 403 : commentaire non accessible

        return await next(context);
    }
}
```

> **Garde-fou** : un filtre par ressource. Pas de `ResourceAuthorizationFilter<T>` générique, pas de `ResourceResolver<T>`, pas de pipeline abstrait.

### 6.6 Configuration JWT + Authorization (`Program.cs`)

```csharp
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;   // requis (package + using)
using Microsoft.IdentityModel.Tokens;

var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = jwt["SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey n'est pas configurée.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30),
            // INDISPENSABLE pour que User.IsInRole(...) fonctionne :
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization(options =>
{
    // --- Policies de PERMISSION (endpoint-level) ---
    options.AddPolicy(Policies.Events.Read, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Read)));
    options.AddPolicy(Policies.Events.Create, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Create)));
    options.AddPolicy(Policies.Events.Update, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Update)));
    options.AddPolicy(Policies.Events.Confirm, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Confirm)));
    options.AddPolicy(Policies.Events.Cancel, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Cancel)));
    options.AddPolicy(Policies.Events.Close, p => p.Requirements.Add(new PermissionRequirement(Permissions.Events.Close)));
    options.AddPolicy(Policies.Comments.Read, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Read)));
    options.AddPolicy(Policies.Comments.Create, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Create)));
    options.AddPolicy(Policies.Comments.Update, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Update)));
    options.AddPolicy(Policies.Comments.Delete, p => p.Requirements.Add(new PermissionRequirement(Permissions.Comments.Delete)));

    // --- Policies de RESSOURCE (usage exclusif avec AuthorizeAsync) ---
    options.AddPolicy(ResourceOperations.Events.Manage, p => p.Requirements.Add(new EvenementResourceRequirement()));
    options.AddPolicy(ResourceOperations.Comments.Manage, p => p.Requirements.Add(new CommentaireResourceRequirement()));
});

// Services de lecture légère
builder.Services.AddScoped<IEvenementAuthorizationReadService, EvenementAuthorizationReadService>();
builder.Services.AddScoped<ICommentaireAuthorizationReadService, CommentaireAuthorizationReadService>();

// Handlers d'autorisation
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, EvenementResourceAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, CommentaireResourceAuthorizationHandler>();

// Filtres
builder.Services.AddScoped<EvenementResourceAuthorizationFilter>();
builder.Services.AddScoped<CommentaireResourceAuthorizationFilter>();

// Émission de jetons (v3) — voir §6.9
builder.Services.AddSingleton<JwtTokenFactory>();

// ... reste de la configuration (DbContext, repositories, handlers, validators, ProblemDetails) ...

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();      // AVANT les endpoints
app.UseStatusCodePages();

app.UseAuthentication();        // 1) décode le JWT → ClaimsPrincipal   (ORDRE STRICT)
app.UseAuthorization();         // 2) évalue les policies

app.MapEventEndpoints();
app.MapCommentEndpoints();
app.MapUserEndpoints();         // v3.1 : /api/me (+ /dev/token en Development)
```

> **Rappel critique :** `app.UseAuthentication()` puis `app.UseAuthorization()` sont **obligatoires** et doivent précéder le mapping des endpoints. Sans eux, `http.User` reste anonyme : tout endpoint `RequireAuthorization()` renvoie **401** en permanence, et `GetUserId()` lève `MissingUserClaimException`.
>
> **Piège d'installation :** `JwtBearerDefaults` n'existe que si le package `Microsoft.AspNetCore.Authentication.JwtBearer` est référencé. En .NET 8, il ne fait pas partie du SDK :
>
> ```powershell
> dotnet add src/EventManagement.Api package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.*
> ```

### 6.7 `appsettings.json` + User Secrets

```json
{
  "Jwt": {
    "Issuer": "EventManagementLab",
    "Audience": "EventManagementLab",
    "SigningKey": ""
  }
}
```

```powershell
dotnet user-secrets init --project src/EventManagement.Api
dotnet user-secrets set "Jwt:SigningKey" "<clé >= 32 caractères>" --project src/EventManagement.Api
```

### 6.8 Ajout v3 — `Roles` et `RolePermissions` (mapping rôle → permissions)

```csharp
// Api/Security/TokenIssuance/Roles.cs
namespace EventManagement.Api.Security.TokenIssuance;

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Organizer     = "Organizer";
    public const string Participant   = "Participant";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string> { Administrator, Organizer, Participant };
}
```

```csharp
// Api/Security/TokenIssuance/RolePermissions.cs
namespace EventManagement.Api.Security.TokenIssuance;

using EventManagement.Application.Authorization;

public static class RolePermissions
{
    // Gestion complète des événements + commentaires.
    private static readonly string[] EventManager =
    [
        Permissions.Events.Read,  Permissions.Events.Create, Permissions.Events.Update,
        Permissions.Events.Confirm, Permissions.Events.Cancel, Permissions.Events.Close,
        Permissions.Comments.Read, Permissions.Comments.Create,
        Permissions.Comments.Update, Permissions.Comments.Delete
    ];

    // Consultation des événements + commentaires (et gestion de SES commentaires).
    private static readonly string[] Participant =
    [
        Permissions.Events.Read,
        Permissions.Comments.Read, Permissions.Comments.Create,
        Permissions.Comments.Update, Permissions.Comments.Delete
    ];

    public static IReadOnlyList<string> For(string role) => role switch
    {
        Roles.Administrator => EventManager,
        Roles.Organizer     => EventManager,
        Roles.Participant   => Participant,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Rôle inconnu.")
    };
}
```

> **Lecture du mapping :** `Administrator` et `Organizer` portent les **mêmes** permissions. Ce qui les distingue n'est **pas** le jeton de permission, mais le **bypass ressource** `IsInRole("Administrator")` (voir §1.3). Les rôles ne sont donc pas trois ensembles de permissions disjoints : ce sont des **personas** dont la différence se joue à deux endroits précis (permissions + bypass ressource).

### 6.9 Ajout v3 — `JwtTokenFactory` (émission des jetons)

```csharp
// Api/Security/TokenIssuance/JwtTokenFactory.cs
namespace EventManagement.Api.Security.TokenIssuance;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public sealed class JwtTokenFactory(IConfiguration configuration)
{
    public string Create(Guid userId, string role, TimeSpan? lifetime = null)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SigningKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Role, role)                      // → IsInRole(...)
        };
        claims.AddRange(RolePermissions.For(role).Select(p => new Claim("permission", p)));

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromHours(2)),   // TOUJOURS UtcNow
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**Où l'utiliser ?** Deux usages acceptables pour le labo, à choisir :

| Option | Où | Garde-fou |
|---|---|---|
| **Helper de tests** (recommandé) | Projet de tests d'intégration | Rien à durcir ; jamais déployé |
| **Endpoint `/dev/token?role=`** | `UserEndpoints.cs`, **uniquement en Development** | Gardé par `if (app.Environment.IsDevelopment())` ; **jamais** en Production |

> **Interdit :** un endpoint d'émission de jetons accessible en Production sans authentification. C'est un générateur d'identités illimité.

### 6.10 Note v3 — pourquoi `RoleClaimType` garantit `IsInRole`

`JwtSecurityTokenHandler` applique par défaut un **mapping de types de claims** à la validation. Selon ce mapping, un claim de rôle peut atterrir sous un type court (`role`) plutôt que long. Déclarer explicitement :

```csharp
RoleClaimType = ClaimTypes.Role
NameClaimType = ClaimTypes.NameIdentifier
```

force le `ClaimsPrincipal` à exposer le rôle **sous le type** que `IsInRole(...)` consulte. Sans cela, `IsInRole("Administrator")` renvoie **silencieusement `false`** : l'admin ne bénéficie plus de son bypass ressource, et le bug est invisible (aucune exception).

---

## 7. Patron d'endpoint (référence)

> **Règle de rangement (v3.1) :** un fichier par **ressource exposée**. Les endpoints d'une même ressource sont regroupés dans un `MapGroup` unique et enregistrés par une extension `MapXxxEndpoints()`.

### 7.1 Update (permission + ressource + domaine)

```csharp
// Api/Endpoints/EventEndpoints.cs
var group = app.MapGroup("/api/events")
    .WithTags("Events")
    .RequireAuthorization();   // Deny by default

group.MapPut("/{id:guid}", async (
    Guid id,
    UpdateEvenementRequest request,
    IValidator<UpdateEvenementCommand> validator,
    UpdateEvenementCommandHandler handler,
    CancellationToken ct) =>
{
    var command = new UpdateEvenementCommand(id, request.Titre, request.Description, request.PersonneImpliquee);

    var validation = await validator.ValidateAsync(command, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    await handler.Handle(command, ct);   // le domaine lève DomainException → 409
    return Results.NoContent();
})
.RequireAuthorization(Policies.Events.Update)                       // 401 / 403 (permission)
.AddEndpointFilter<EvenementResourceAuthorizationFilter>();          // 404 / 403 (ressource)
```

### 7.2 Create (identité en entrée)

```csharp
group.MapPost("/", async (
    CreateEventRequest request,
    IValidator<CreateEventCommand> validator,
    CreateEventCommandHandler handler,
    HttpContext http,
    CancellationToken ct) =>
{
    // L'identité traverse la frontière EN VALEUR.
    var command = new CreateEventCommand(
        request.Titre, request.Description, request.PersonneImpliquee,
        http.User.GetUserId());

    var validation = await validator.ValidateAsync(command, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var id = await handler.Handle(command, ct);
    return Results.Created($"/api/events/{id}", new { id });
})
.RequireAuthorization(Policies.Events.Create);
```

### 7.3 AddComment (identité en entrée, autorisation sur l'événement)

```csharp
// Api/Endpoints/CommentEndpoints.cs — sous le groupe /api/events/{eventId:guid}/comments
group.MapPost("/", async (
    Guid eventId,
    AddCommentRequest request,
    IValidator<AddCommentCommand> validator,
    AddCommentCommandHandler handler,
    HttpContext http,
    CancellationToken ct) =>
{
    var command = new AddCommentCommand(eventId, request.Contenu, http.User.GetUserId());

    var validation = await validator.ValidateAsync(command, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    var id = await handler.Handle(command, ct);
    return Results.Created($"/api/events/{eventId}/comments/{id}", new { id });
})
.RequireAuthorization(Policies.Comments.Create);
```

> **Note :** `AddComment` ne passe **pas** par le filtre de ressource. La capacité à commenter est contrôlée par la **permission globale** `Comments.Create` ; l'autorité finale sur l'état de l'événement reste `Evenement.AjouterCommentaire()` (→ 409 si l'événement est annulé/clôturé).

### 7.4 UpdateComment (autorisation sur le commentaire)

```csharp
group.MapPut("/{commentaireId:guid}", async (
    Guid evenementId, Guid commentaireId, UpdateCommentaireRequest request,
    IValidator<UpdateCommentaireCommand> validator,
    UpdateCommentaireCommandHandler handler, CancellationToken ct) =>
{
    var command = new UpdateCommentaireCommand(evenementId, commentaireId, request.Contenu);
    var validation = await validator.ValidateAsync(command, ct);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    await handler.Handle(command, ct);
    return Results.NoContent();
})
.RequireAuthorization(Policies.Comments.Update)
.AddEndpointFilter<CommentaireResourceAuthorizationFilter>();   // 404 / 403 (Auteur || Admin)
```

### 7.5 Ajout v3 / corrigé v3.1 — endpoint de profil `/me`

**Emplacement : `Api/Endpoints/UserEndpoints.cs`.**

```csharp
// Api/Endpoints/UserEndpoints.cs
using EventManagement.Api.Security;   // GetUserId(), GetRoles()

namespace EventManagement.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/me")
            .WithTags("Me")
            .RequireAuthorization();   // deny by default : « authentifié » suffit

        // "" (et non "/") pour viser exactement /api/me, sans slash terminal.
        group.MapGet("", (HttpContext http) => Results.Ok(new MeResponse(
            UserId: http.User.GetUserId(),
            Roles:  http.User.GetRoles())));

        return app;
    }
}

public sealed record MeResponse(Guid UserId, IReadOnlyList<string> Roles);
```

**Pourquoi ce fichier et pas un autre :**

| Emplacement | Verdict | Raison |
|---|---|---|
| `EventEndpoints.cs` | ❌ | Le fichier sert la ressource *Événement* ; `/me` n'y touche pas |
| `CommentEndpoints.cs` | ❌ | Idem, pour la ressource *Commentaire* |
| **`UserEndpoints.cs`** | ✅ | Sert l'**identité** de l'appelant : c'est une ressource à part entière |

**Pourquoi le groupe `/api/me` et non `/api/users/me` :** la variante imbriquée entrerait en collision visuelle avec une future route `/api/users/{id:guid}`. `/api/me` est une convention reconnue et évite la question.

**Pourquoi `MeResponse` est un record nommé :** Swagger génère alors un schéma exploitable, là où un objet anonyme produit un schéma vide.

**Ce qu'il ne faut PAS y mettre :**

| À éviter | Raison |
|---|---|
| `AddEndpointFilter<...ResourceAuthorizationFilter>()` | Aucune ressource à charger : pas de projection, pas de 404 possible |
| Un `GetMeQueryHandler` CQRS | La donnée vient du **jeton**, pas de la base : aucune requête à émettre |
| Un `http.User.IsInRole(...)` | Règle du labo : aucune **décision** de rôle hors `Api/Security` |
| Une policy de permission | `.RequireAuthorization()` sans policy = « authentifié suffit », exactement le besoin |

> **Précision importante :** `GetRoles()` **expose** une donnée, il ne **décide** rien. Le critère « aucun `IsInRole` hors `Api/Security` » vise `IsInRole` en tant que *décision* : il reste respecté. C'est la distinction « informer vs autoriser ».

**Emplacement de `/dev/token` (si implémenté) :** ce même fichier, sous `if (app.Environment.IsDevelopment())`. C'est aussi de l'identité.

### 7.6 Ajout v3.1 — variante examinée et écartée

**Variante :** refuser au niveau du middleware les jetons dépourvus de claim `sub`, via `JwtBearerEvents.OnTokenValidated` :

```csharp
// VARIANTE ÉCARTÉE — conservée ici pour mémoire du raisonnement
options.Events = new JwtBearerEvents
{
    OnTokenValidated = ctx =>
    {
        if (!Guid.TryParse(ctx.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out _))
            ctx.Fail("Jeton sans identité exploitable.");
        return Task.CompletedTask;
    }
};
```

**Séduisante sur le principe** : un jeton sans identité est un échec d'*authentification*, donc le 401 serait produit par le middleware, au bon endroit sémantique, et aucun endpoint ne verrait jamais un principal « à moitié valide ».

**Écartée pour deux raisons :**

1. **Redondance.** Le garde-fou de `GetUserId()` reste nécessaire de toute façon (défense en profondeur : un bug de configuration ne doit pas produire un 500). Ajouter `OnTokenValidated` instaurerait **deux** mécanismes pour une seule garantie — contraire à la règle « un seul endroit par règle » (§14.4).
2. **Point de traduction unique.** La table §10 fait du `GlobalExceptionHandler` le **seul** traducteur vers HTTP. Produire un 401 supplémentaire depuis `OnTokenValidated` affaiblit cette invariante pédagogique sans bénéfice démontré.

**Décision :** on conserve **un** mécanisme — `MissingUserClaimException` mappée en 401 (§10). Si un jour le besoin d'un rejet *précoce* se manifeste (ex. : coût des requêtes sur jetons malformés), la variante sera réévaluée à ce moment-là.

---

## 8. Matrice des droits (référence)

### 8.1 Permission / Ressource / Domaine

| Opération | HTTP | Permission | Resource | Domaine |
|---|---|---|---|---|
| Create Event | POST → **201** | `Events.Create` | — | `Evenement.Create` |
| Read Events | GET → **200** | `Events.Read` | — | — |
| Update Event | PUT → **204** | `Events.Update` | Owner \|\| Admin | `PeutEtreModifie` |
| Confirm Event | POST → **204** | `Events.Confirm` | Owner \|\| Admin | `Draft → Confirmed` |
| Cancel Event | POST → **204** | `Events.Cancel` | Owner \|\| Admin | `≠ Cloture` |
| Close Event | POST → **204** | `Events.Close` | Owner \|\| Admin | `Confirmed → Cloture` |
| Read Comments | GET → **200** | `Comments.Read` | — | — |
| Create Comment | POST → **201** | `Comments.Create` | — (permission seule) | `AjouterCommentaire` |
| Update Comment | PUT → **204** | `Comments.Update` | Auteur \|\| Admin | État Event/Comment |
| Delete Comment | DELETE → **204** | `Comments.Delete` | Auteur \|\| Admin | État Event/Comment |
| **Read Me** | GET → **200** | — (authentifié suffit) | — | — |

### 8.2 Ajout v3 — Rôles de référence (permissions portées + bypass ressource)

| Rôle | Permissions portées par le jeton | Bypass ressource (`IsInRole`) |
|---|---|---|
| **Administrator** | toutes (`Events.*`, `Comments.*`) | ✅ oui — agit sur toute ressource |
| **Organizer** | toutes (`Events.*`, `Comments.*`) | ❌ non — propriétaire / auteur seulement |
| **Participant** | `Events.Read`, `Comments.Read/Create/Update/Delete` | ❌ non — auteur seulement |

**Matrice role × opération** (le ✅ d'Administrator suppose que son jeton porte **bien** la permission — cf. §1.3) :

| Rôle | Créer évt | Lire évt | Modifier/Annuler un évt | Commenter | Modifier/supprimer un commentaire |
|---|---|---|---|---|---|
| Administrator | ✅ | ✅ | ✅ (tous) | ✅ | ✅ (tous) |
| Organizer | ✅ | ✅ | ✅ (les siens) | ✅ | ✅ (les siens) |
| Participant | ❌ | ✅ | ❌ | ✅ | ✅ (les siens) |

Codes de statut :

```text
401 = authentification absente/invalide (ou identité inexploitable dans le jeton)
403 = authentifié mais autorisation insuffisante (permission OU ressource)
404 = ressource inexistante
409 = ressource existante mais opération incompatible avec l'état métier
```

---

## 9. Diagrammes de séquence

### 9.1 `PUT /api/events/{id}`

```text
Client ── PUT /api/events/42 ──► API
                                  │
                                  ├─ JWT valide ? ── NON ──► 401   (middleware auth)
                                  │
                                  ├─ Permission Events.Update ? ── NON ──► 403
                                  │
                                  ├─ Filter : lit {Id, ProprietaireId} (projection)
                                  │      └─ inexistant ? ──► 404
                                  │      └─ Owner||Admin ? ── NON ──► 403
                                  │
                                  ├─ Validation FluentValidation ? ── NON ──► 400
                                  │
                                  ├─ UpdateEvenementCommandHandler
                                  │      └─ charge agrégat complet
                                  │      └─ Evenement.Modifier(...)
                                  │             └─ PeutEtreModifie() ? NON ──► 409
                                  │
                                  └─ EF Core SaveChanges ──► 204
```

### 9.2 `POST /api/events/{id}/confirm`

```text
Client ── POST /api/events/42/confirm ──► API
     JWT ──► Permission Events.Confirm ──► Filter (Owner||Admin, policy Event.Manage) ──►
     ConfirmEvenementCommandHandler ──► Evenement.Confirmer()
         └─ Draft → Confirmed ? sinon DomainException ──► 409
     ──► 204
```

### 9.3 `PUT /api/events/{eventId}/comments/{commentaireId}`

```text
Client ── PUT .../comments/7 ──► API
     JWT ──► Permission Comments.Update ──►
     Filter : lit {Id, EvenementId, AuteurId} (route "commentaireId")
         └─ inexistant ? ──► 404
         └─ Auteur||Admin ? ── NON ──► 403
     UpdateCommentaireCommandHandler ──► Evenement.ModifierCommentaire(...)
         └─ état Event invalide ? ──► 409
     ──► 204
```

### 9.4 Ajout v3 — identification du rôle (émission puis lecture)

```text
Connexion (dev/test)
   JwtTokenFactory.Create(userId, "Organizer")
      ├─ claim sub        = userId
      ├─ claim role       = "Organizer"           (ClaimTypes.Role)
      └─ claim permission = Events.* + Comments.*  (via RolePermissions.For)

Requête protégée
   JWT décodé ──► RoleClaimType = ClaimTypes.Role
      ├─ IsInRole("Administrator") ? non  → pas de bypass ressource
      ├─ permissions du jeton    → étape « permission »
      └─ GetRoles() (endpoint /me) → ["Organizer"]
```

### 9.5 Ajout v3.1 — `GET /api/me`

```text
Client ── GET /api/me ──► API
     JWT valide ? ── NON ──► 401
     ├─ claim sub présent et parsable en Guid ?
     │      └─ NON ──► MissingUserClaimException ──► 401   (et non 500)
     └─ OUI ──► 200 { userId, roles }
     (aucune policy, aucun filtre de ressource, aucune lecture base)
```

---

## 10. Traduction des erreurs (`GlobalExceptionHandler`)

Les handlers d'autorisation ne produisent **jamais** de code HTTP. Le pipeline les traduit :

| Origine | Exception / résultat | Code HTTP |
|---|---|---|
| Middleware auth (JWT) | échec d'authentification | **401** |
| **Identité** | **`MissingUserClaimException`** (claim `sub`/`NameIdentifier` absent ou invalide) | **401** |
| Policy de permission | échec d'autorisation | **403** |
| Filtre de ressource | `Results.NotFound()` | **404** |
| Filtre de ressource | `Results.Forbid()` | **403** |
| Validation | `Results.ValidationProblem()` | **400** |
| Application | `NotFoundException` | **404** |
| Domaine | `DomainException` | **409** |
| Inattendu | toute autre exception | **500** |

> **Ajout v3.1 — ligne manquante.** La v3 documentait `GetUserId()` levant `UnauthorizedAccessException` **sans** mapper cette exception. Conséquence : un jeton *signé correctement mais dépourvu du claim `sub`* franchissait le middleware (401 écarté), puis faisait échouer `GetUserId()` → **500**, sémantiquement faux. Le cas est désormais explicite : exception **dédiée** (§6.2) → **401**.

```csharp
// Api/Middleware/GlobalExceptionHandler.cs — extrait
private static int ToStatusCode(Exception ex) => ex switch
{
    MissingUserClaimException => StatusCodes.Status401Unauthorized,   // identité inexploitable
    DomainException           => StatusCodes.Status409Conflict,       // état métier incompatible
    NotFoundException         => StatusCodes.Status404NotFound,       // ressource absente
    _                         => StatusCodes.Status500InternalServerError
};
```

> **Pourquoi ne pas mapper `UnauthorizedAccessException` directement ?** Ce type est aussi levé par le runtime pour des accès fichiers/OS refusés. Un mapping global vers 401 masquerait ces erreurs réelles derrière un code d'authentification trompeur. Le type dédié supprime l'ambiguïté.

---

## 11. Tests

### 11.1 Tests unitaires des resource handlers (sans HTTP)

```csharp
[Fact]
public async Task EvenementResource_Succeeds_WhenOwner()
{
    var ownerId = Guid.NewGuid();
    var info = new EvenementAuthorizationInfo(Guid.NewGuid(), ownerId);

    var user = new ClaimsPrincipal(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, ownerId.ToString())], "Test"));

    var context = new AuthorizationHandlerContext([new EvenementResourceRequirement()], user, info);
    var handler = new EvenementResourceAuthorizationHandler();
    await handler.HandleAsync(context);

    context.HasSucceeded.Should().BeTrue();
}

[Fact]
public async Task EvenementResource_Succeeds_WhenAdministrator()
{
    var info = new EvenementAuthorizationInfo(Guid.NewGuid(), Guid.NewGuid());   // autre propriétaire
    var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Administrator")     // bypass ressource
        ], "Test"));

    var context = new AuthorizationHandlerContext([new EvenementResourceRequirement()], user, info);
    var handler = new EvenementResourceAuthorizationHandler();
    await handler.HandleAsync(context);

    context.HasSucceeded.Should().BeTrue();
}
```

> **Note :** la `ClaimsIdentity` doit recevoir un `authenticationType` (ici `"Test"`) et son `RoleClaimType` par défaut est bien `ClaimTypes.Role` — c'est ce qui permet à `IsInRole("Administrator")` de fonctionner dans le test.

### 11.2 Tests d'intégration (401/403/404/409/200-204)

| Scénario | Attendu |
|---|---|
| `PUT /events/{id}` sans JWT | **401** |
| `PUT /events/{id}` JWT sans permission `Events.Update` | **403** |
| `PUT /events/{id}` permission OK, event inexistant | **404** |
| `PUT /events/{id}` permission OK, event d'un autre (non Admin) | **403** |
| `PUT /events/{id}` propriétaire, event `Annule` | **409** |
| `PUT /events/{id}` propriétaire, event `Draft` | **204** |
| `PUT /events/{id}` **Administrator**, event d'un autre | **204** |
| `GET /events/{id}` permission `Events.Read` | **200** |
| `POST /events` permission `Events.Create` | **201** |
| `POST /events` rôle `Participant` (sans `Events.Create`) | **403** |
| `PUT /events/{eid}/comments/{cid}` JWT, non auteur (non Admin) | **403** |
| `PUT /events/{eid}/comments/{cid}` auteur | **204** |
| Token admin **sans** claim `permission` | **403** (piège §1.3) |
| **`GET /me`** sans JWT | **401** |
| **`GET /me`** JWT valide (rôle `Organizer`) | **200** + `{ userId, roles: ["Organizer"] }` |
| **`GET /me`** JWT signé **sans** claim `sub` | **401** (et **non 500** — cf. §10) |

---

## 12. Critères d'acceptation

- [ ] Tout endpoint est protégé par défaut (`RequireAuthorization()` sur le groupe).
- [ ] Aucun `AuthorizationHandler` ne référence le `Statut`.
- [ ] Aucun `User.IsInRole(...)` en dehors de `Api/Security`.
- [ ] L'identité circule par **valeur** dans les commands, jamais via `IHttpContextAccessor`.
- [ ] `Commentaire.AuteurId` exploité avec un handler dédié.
- [ ] Clé JWT hors du code source (config + User Secrets).
- [ ] `RoleClaimType = ClaimTypes.Role` configuré explicitement.
- [ ] Codes HTTP strictement sémantiques (401/403/404/409/400/200/201/204).
- [ ] Les filtres de ressource font une **projection légère** (`Id`, `ProprietaireId`/`AuteurId`), pas un chargement d'agrégat.
- [ ] Chaque filtre lit **sa** clé de route (`id` pour les événements, `commentaireId` pour les commentaires).
- [ ] Une **seule** policy de ressource par agrégat (`Event.Manage`, `Comment.Manage`).
- [ ] Double lecture (filtre léger → handler complet) documentée comme concession du Labo 2.
- [ ] Les rôles vivent dans `Api/Security`, jamais dans `Application` ni `Domain`.
- [ ] Aucun rôle n'est stocké en base ; le mapping rôle → permissions est code-defined.
- [ ] Le jeton d'un `Administrator` porte **toutes** les permissions (cf. §1.3).
- [ ] Aucun endpoint d'émission de jetons n'est actif hors `Development`.
- [ ] `GetRoles()` n'est utilisé que pour **informer**, jamais pour décider.
- [ ] **(v3.1)** L'endpoint `/me` vit dans `Api/Endpoints/UserEndpoints.cs`, jamais dans `EventEndpoints.cs` ni `CommentEndpoints.cs`.
- [ ] **(v3.1)** `/me` n'a **ni** filtre de ressource, **ni** policy de permission, **ni** handler CQRS.
- [ ] **(v3.1)** `GetUserId()` lève `MissingUserClaimException`, **jamais** `UnauthorizedAccessException`.
- [ ] **(v3.1)** `MissingUserClaimException` est mappée en **401** ; aucun chemin ne produit un 500 pour un défaut d'identité.
- [ ] **(v3.1)** `app.UseAuthentication()` **puis** `app.UseAuthorization()` précèdent le mapping des endpoints.
- [ ] **(v3.1)** Le package `Microsoft.AspNetCore.Authentication.JwtBearer` est référencé par `EventManagement.Api`.

---

## 13. Garde-fous (ce qu'on ne fait PAS)

```text
❌ AspNetUsers / AspNetRoles / table de permissions (Labo 3 si besoin réel)
❌ IPermissionService / SecurityContext / AuthorizationFacade
❌ IHttpContextAccessor dans les handlers métier
❌ Rôles, claims ou HttpContext dans le Domain
❌ Delete métier pour Evenement (→ Cancel)
❌ Autorisation frontend comme mécanisme de sécurité
❌ ResourceAuthorizationFilter<T> générique / mini-framework d'autorisation
❌ Une policy de ressource par opération quand la règle est identique
❌ Un rôle comme couche d'autorisation supplémentaire (le rôle = contenu du jeton)
❌ Entity Role / RoleRepository / table de rôles (→ Labo 3 si besoin réel)
❌ Mapping rôle → permissions en base ou réparti dans plusieurs classes
❌ Endpoint d'émission de jetons actif en Production
❌ `IsInRole` pour décider autre chose que le bypass ressource Admin
❌ (v3.1) `/me` rangé dans `EventEndpoints.cs` ou `CommentEndpoints.cs` « faute de mieux »
❌ (v3.1) Filtre de ressource ou policy de permission sur `/me` (aucune ressource à charger)
❌ (v3.1) Handler CQRS pour `/me` (la donnée vient du jeton, pas de la base)
❌ (v3.1) `UnauthorizedAccessException` mappée globalement en 401 (masque des erreurs réelles)
❌ (v3.1) Deux mécanismes pour une garantie (ex. `OnTokenValidated` + exception dédiée)
❌ xmin / RowVersion / concurrence optimiste (→ Labo 3)
❌ OrganizationId / RBAC multi-tenant (→ Labo 3)
```

---

## 14. Notes pédagogiques

1. **Double lecture assumée** : le filtre lit une projection légère, le handler lit l'agrégat complet. Deux lectures, deux formes, deux finalités (décider vs agir). Ce n'est pas un défaut : c'est la séparation CQRS.

2. **Ordre d'exécution fixe** : `UseAuthentication` → `UseAuthorization` → filtre d'endpoint → handler. On ne peut pas obtenir 404/403 « ressource » sans être déjà passé par 401/403 « permission ».

3. **Sémantique HTTP stricte** : les handlers d'autorisation ne renvoient jamais de code HTTP directement ; c'est le pipeline qui les traduit (voir §10).

4. **Un seul endroit par règle** : si demain la règle « Propriétaire » change, elle se modifie dans **un** handler. Si la règle « permission » change, elle se modifie dans **une** policy. Corollaire (v3.1) : si la règle « identité exploitable » change, elle se modifie dans **un** type d'exception.

5. **Le rôle est un *bundle*, pas une couche.** Dans ce labo, un rôle ne fait que **remplir un jeton** : il choisit un ensemble de permissions, et éventuellement le bit `IsInRole`. Il n'ajoute aucun chemin de code dans l'API. Le présenter comme une « troisième couche » d'autorisation serait une erreur de lecture du modèle.

6. **La différence Organizer/Administrator est instructive.** Mêmes permissions, comportements différents : c'est la preuve que **permission ≠ autorisation ressource**. Seul l'Admin franchit la barrière du propriétaire, *via* le bypass ressource.

7. **(v3.1) L'organisation des fichiers suit la ressource, pas la commodité.** `/me` ne sert aucune ressource métier : il obtient son propre fichier. La règle « un fichier par ressource exposée » est ce qui empêche un `EventEndpoints.cs` de devenir le refuge de tout ce qui ne va nulle part ailleurs.

8. **(v3.1) Une identité inexploitable est un échec d'authentification, pas une erreur serveur.** Un 500 sur un jeton sans `sub` est un bug de contrat : le code doit dire *qui* a échoué et *pourquoi*, avec un 401 explicite.

---

## 15. Journal des corrections

### 15.1 v1 → v2

| # | Défaut v1 | Gravité | Correction v2 |
|---|---|---|---|
| 1 | Le filtre autorisait en dur contre `Event.Update.Resource` pour toutes les opérations | 🔴 Bug | Policy unique par agrégat : `Event.Manage` |
| 2 | Quatre policies de ressource identiques | 🟠 Redondance | Une seule : `Event.Manage` |
| 3 | Le filtre de commentaires lisait `RouteValues["id"]` au lieu de `commentaireId` | 🔴 Bug | Lecture de `commentaireId` |
| 4 | `CommentaireResourceAuthorizationFilter` référencé mais non fourni | 🟠 Manque | Code fourni (§6.5) |
| 5 | Mapping HTTP des exceptions non explicite | 🟠 Manque | Table de traduction (§10) |
| 6 | Flux d'identité vers les commands non montré | 🟠 Manque | Exemples §7.2-7.3 |
| 7 | Ordre middleware → filtre → handler implicite | 🟡 Implicite | Explicité |
| 8 | `Guid.TryParse` redondant avec `{id:guid}` | 🟡 Cosmétique | Simplifié |

### 15.2 v2 → v3

| # | Défaut v2 | Gravité | Correction v3 |
|---|---|---|---|
| 9 | Les rôles n'étaient pas définis ; seul `Administrator` apparaissait, sans cartographie | 🟠 Manque | 3 rôles + mapping rôle → permissions (§6.8) |
| 10 | Aucun moyen d'émettre un jeton conforme (ni helper, ni fabrique) | 🟠 Manque | `JwtTokenFactory` (§6.9) |
| 11 | L'idée fausse « Admin passe sans condition » n'était pas démentie | 🔴 Piège | `Administrator` a **aussi** besoin des permissions (§1.3) |
| 12 | La lecture des rôles (`GetRoles`) n'existait pas | 🟡 Manque | Extension + endpoint `/me` (§6.1, §7.5) |
| 13 | `IsInRole("Administrator")` en chaîne magique | 🟡 Cosmétique | Constante `Roles.Administrator` |
| 14 | Le rôle risquait d'être lu comme une couche d'autorisation | 🟠 Sémantique | Cadrage « bundle de permissions » (§1.2, §14.5) |
| 15 | Le piège du mapping de claims (`RoleClaimType`) n'était pas relié | 🟡 Implicite | Note dédiée (§6.10) |

### 15.3 v3 → v3.1

| # | Défaut v3 | Gravité | Correction v3.1 |
|---|---|---|---|
| 16 | L'emplacement de `/me` était indéterminé (risque de rangement dans `EventEndpoints`) | 🟠 Contrat | `Api/Endpoints/UserEndpoints.cs`, groupe `/api/me` (§1.4, §7.5) |
| 17 | `GetUserId()` levait `UnauthorizedAccessException`, **non mappée** → 500 sur défaut d'identité | 🔴 Bug | `MissingUserClaimException` dédiée, mappée **401** (§6.1, §6.2, §10) |
| 18 | L'arborescence §2 omettait `Endpoints/` et `Middleware/`, pourtant cités ailleurs | 🟠 Cohérence | Arborescence complétée + règle d'organisation explicite (§2) |
| 19 | La variante « rejet au niveau du middleware » n'était pas arbitrée | 🟡 Manque | Variante documentée puis écartée, avec motif (§7.6) |
| 20 | Le namespace des read services était `Infrastructure.Queries` au lieu de `Infrastructure.QueryServices` | 🟡 Incohérence | Aligné sur le projet réel (§5, §2) |
| 21 | Aucun critère d'acceptation ne couvrait `/me` ni l'ordre du pipeline | 🟡 Manque | 6 critères ajoutés (§12) |
| 22 | `GetUserId()` utilisait `JwtRegisteredClaimNames` sans le `using` correspondant (exemple non compilable) | 🟡 Cohérence | `using System.IdentityModel.Tokens.Jwt;` ajouté (§6.1) |
