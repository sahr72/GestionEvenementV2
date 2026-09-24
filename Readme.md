1. Introduction
1.1 Objectif de travail
Construire une API REST Entreprise de gestion d'événements conforme à :

Architecture propre ;
DDD tactique léger ;
CQRS pragmatique sans MediatR;
Domaine riche protégeant les invariants ;
EF Core 8 + PostgreSQL + Docker ;
API minimale ;
Gestion homogène des erreurs via ProblemDetails;
Tests automatisés (unitaire, applicatif, intégration).

Regles de gestion (Elles sont appliquees dans les Aggregats du domaine, pas dans l'Application.)
1.Seul un événement en brouillon peut être confirmé.
2.Un événement clôturé ne peut pas être annulé.On empêche les transitions invalides (ex : Cloture→ Brouillon).
3.Seul un événement confirmé peut être clôturé.On empêche les transitions invalides (ex : Cloture→ Brouillon).
4.Les transitions d'état sont contrôlées par des méthodes métier ( Confirmer, Annuler, Cloturer).
2. Règle d'unicité et de chronologie des commentaires
2.1 Ajout d'une date de fin à l'événement (optionnel mais cohérent)
2.2 Règle : pas de commentaire si événement annulé ou clôturé
2.3 Règle : pas de commentaire daté dans le futur: Remarque : À utiliser DateTime.UtcNowpour éviter les problèmes de fuseaux horaires. En entreprise, on pourrait injecter un IDateTimeProvidersi on veut tester/mocker la date.
3. Règle de gestion des doublons de commentaires
3.1 Ajout d'un auteur (ou identifiant utilisateur) au commentaire
3.2 Règle : pas de doubleblon (même auteur + même description à la même minute)

Creer les classes de domaine
Une fois le Domaine modélisé, la prochaine étape logique en entreprise serait :

Creer les ReadModels(DTO de sortie)
Créez les interfaces de l'Application :( IEventRepository, IEventQueryService)+ Creer IEvenementQueryService  (port pour les lectures) 
Implémenter les Command Handlers dans EventManagement.Application.Command
Implémenter les Query Handlers dans EventManagement.Application.Queries avec des ReadModels.
Séparation des responsabilités : 
Les Handlers délèguent au Domaine (Commands) ou aux Query Services (Requêtes). 
Pas de métier logique dans l'Application.
Abstractions justifiées : IEventRepository protège le Domaine de la persistance. 
IEventQueryService protège l'Application des détails de lecture (EF Core, SQL).
Simplicité : Pas de MediatR, pas de surcouche inutile. Les Handlers sont des classes simples avec une méthode Handle.
Préparation pour les tests : Les handlers sont facilement testables avec des mocks de IEventRepositoryet IEventQueryService.

Test du domaine:
Objectif : Valider les invariants et comportements du domaine.
Étape 1 – Ajouter FluentAssertions:  dotnet add tests/EventManagement.Domain.Tests package FluentAssertions
  
  
La prochaine étape logique en entreprise serait :


TP 1.4 – Configuration d'EF Core et de PostgreSQL Docker
Objectif : Configurer la persistance avec EF Core 8 et PostgreSQL via Docker.
Étape 1 – Ajouter les packages
dotnet add src/EventManagement.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/EventManagement.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/EventManagement.Infrastructure package Microsoft.EntityFrameworkCore.Design
Étape 2 – Créer AppDbContext dans EventManagement.Infrastructure.Persistence;
Étape 3 – ConfigurationEvenement et ConfigurationCommentaire
Étape 4 – Docker Compose PostgreSQL: Créer docker-compose.ymlà la racine :
 docker compose up -d
 Verification: docker ps
Étape 5 – Caine de connexion : Dans src/EventManagement.Api/appsettings.Development.json:
  {
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=evenementV2db;Username=eventuser;Password=eventpassword"
  }
}
Étape 6 – Injection de dépendances Dans src/EventManagement.Api/Program.cs:
Configurer l'injection de suspensions dans Program.csl'API.

TP 1.5 – Points de terminaison Premiers API minimale
Créez les endpoints Minimal API qui appellent ces Handlers.
Objectif : Exposer des endpoints basiques pour créer et récupérer un événement.
Étape 1 – Implementer les Interfaces de l'Application dans EventManagement.Infrastructure.Repositories;
EventRepository : IEventRepository pour les operations CRUD
EventQueryService : IEvenementQueryService pour les GET
Injection des dependances de toutes les interfaces dans program.cs

Étape 6 – Migrations et base de données


GetEventsPagedQuery
        │
        ▼
EvenementQueryService
        │
        ├── filtre
        ├── Count
        ├── OrderBy
        ├── Skip / Take
        │
        ▼
EventListItemReadModel
        │
        ▼
PagedResult<EventListItemReadModel>
et pour le détail:
GetEventById
      │
      ▼
EventDetailsReadModel
Lab 2.2 – Validation FluentValidation
Objectif : Valider les entrées avec FluentValidation.
Étape 1 – Ajouter FluentValidation (packages nuget dans la couche Application)
dotnet add src/EventManagement.Application package FluentValidation
dotnet add src/EventManagement.Application package FluentValidation.DependencyInjectionExtensions
Étape 2 – Validateurs
Étape 3 – Injection dans l'API
Étape 4 - Créer tous les DTOs dans la couche Application
Étape 5 – Utilisation dans les endpoints: Classe statique séparée par entité,pas dans program.cs
Étape 6 - Mapper les endpoints dans program.cs
Critères d'acceptation
Les validateurs sont injectés et utilisés.
Une requête invalide retourne un 400 Bad Requestavec ProblemDetails.
Les règles de validation (longueur, présence) sont respectées.
Lab 2.3 – Gestion homogène des erreurs
Objectif : Centraliser la gestion des erreurs avec ProblemDetails.
Étape 1 – Ajouter les détails du problème dans program.cs
Étape 2 – Middleware d'exception: // src/EventManagement.Api/Middleware/GlobalExceptionHandler.cs
Critères d'acceptation
Une DomainExceptionretourne un 409 Conflict.
Une NotFoundExceptionretourne un 404 Not Found.
Une erreur inattendue retourne un 500 Internal Server Error.
Toutes les erreurs utilisent ProblemDetails.
TP 2.4 – Tests applicatifs
Objectif : Tester les handlers avec des mocks.
Étape 1 – Ajouter NSubstitute: 
dotnet add tests/EventManagement.Application.Tests package NSubstitute
dotnet add tests/EventManagement.Application.Tests package FluentAssertions
Étape 2 – Test d'un gestionnaire
Critères d'acceptation
Les tests applicatifs passent.
Les mocks sont utilisés pour isoler le domaine.
Les cas d'erreur (ex: événement introuvable) sont testés.
6. Niveau Avancé — Durcissement et industrialisation