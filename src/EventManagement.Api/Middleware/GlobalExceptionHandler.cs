// src/EventManagement.Api/Middleware/GlobalExceptionHandler.cs

namespace EventManagement.Api.Middleware;

using EventManagement.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
/*
 * Critères d'acceptation

Une DomainExceptionretourne un 409 Conflict.
Une NotFoundExceptionretourne un 404 Not Found.
Une erreur inattendue retourne un 500 Internal Server Error.
Toutes les erreurs utilisent ProblemDetails.
 * */
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ProblemDetails problem;

        if (exception is DomainException domainException)
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