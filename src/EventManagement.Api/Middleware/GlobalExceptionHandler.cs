// src/EventManagement.Api/Middleware/GlobalExceptionHandler.cs

namespace EventManagement.Api.Middleware;

using EventManagement.Api.Security;
using EventManagement.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
/*
 * Critères d'acceptation

Une DomainExceptionretourne un 409 Conflict.
Une NotFoundExceptionretourne un 404 Not Found.
Une erreur inattendue retourne un 500 Internal Server Error.
Toutes les erreurs utilisent ProblemDetails.

MissingUserClaimException doit être testée avant DomainException et NotFoundException parce que :

c'est une erreur de couche infrastructure/sécurité ;
elle peut être levée avant même que le code métier ou d'application ne soit atteint ;
elle est plus spécifique que le fallback 500.

L'ordre des branches reflète la hiérarchie des couches :
  401 (infrastructure / sécurité)
→ 404 (application / ressource)
→ 409 (domaine / règle métier)
→ 500 (non géré)

Point additionnel : logging
Pour un labo, le handler est fonctionnellement complet. Cependant, en production, on ajouterait un ILogger pour distinguer :
401 attendus (claim manquant) ;
500 inattendus (vrais bugs à investiguer).
Sans logger, une NullReferenceException ou une InvalidOperationException seront toutes deux masquées sous le même message générique. 
Acceptable pour un labo, mais je dois le nommer.
 * */
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
     HttpContext httpContext,
     Exception exception,
     CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is MissingUserClaimException missingClaim)
        {
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentification requise",
                Detail = missingClaim.Message,
                Type = "https://httpstatuses.io/401"
            };
        }
        else if (exception is DomainException domainException)
        {
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Règle métier violée",
                Detail = domainException.Message,
                Type = "https://httpstatuses.io/409"
            };
        }
        else if (exception is Application.Exceptions.NotFoundException notFound)
        {
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Ressource introuvable",
                Detail = notFound.Message,
                Type = "https://httpstatuses.io/404"
            };
        }
        else
        {
            problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erreur interne",
                Detail = "Une erreur inattendue est survenue.",
                Type = "https://httpstatuses.io/500"
            };
        }

        httpContext.Response.StatusCode = problem.Status ?? 500;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}