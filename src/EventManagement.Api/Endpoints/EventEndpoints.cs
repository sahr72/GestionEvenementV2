// src/EventManagement.Api/Endpoints/EventsEndpoints.cs

namespace EventManagement.Api.Endpoints;

using EventManagement.Api.Security;
using EventManagement.Api.Security.Filters;
using EventManagement.Application.Authorization;
using EventManagement.Application.Commands;
using EventManagement.Application.Commands.CreateEvent;
using EventManagement.Application.Commands.UpdateEvent;
using EventManagement.Application.DTOs;
using EventManagement.Application.Queries;
using EventManagement.Application.Queries.GetEventById;
using EventManagement.Application.Queries.GetEventsPaged;
using FluentValidation;

/*
 * Critères d'acceptation

Les validateurs sont injectés et utilisés.
Une requête invalide retourne un 400 Bad Requestavec ProblemDetails.
Les règles de validation (longueur, présence) sont respectées.
 * */
public static class EventEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events")
            .WithOpenApi()
            .RequireAuthorization();   // Deny by default;

        // Update (permission + ressource + domaine)
        group.MapPut("/{id:guid}", async (
        Guid id,
        UpdateEventRequest request,
        IValidator<UpdateEventCommand> validator,
        UpdateEventCommandHandler handler,
        CancellationToken ct) =>
        {
            var command = new UpdateEventCommand(id, request.Titre, request.Description, request.PersonneImpliquee);

            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            await handler.Handle(command, ct);   // le domaine lève DomainException → 409
            return Results.NoContent();
        })
        .RequireAuthorization(Policies.Events.Update)                       // 401 / 403 (permission)
        .AddEndpointFilter<EvenementResourceAuthorizationFilter>();          // 404 / 403 (ressource)

        // Create (identité en entrée) ; POST /api/events
        group.MapPost("/", async (
            CreateEventRequest request,
            IValidator<CreateEventCommand> validator,
            CreateEventCommandHandler handler,
            HttpContext http,
            CancellationToken ct) =>
        {
            var command = new CreateEventCommand(
                request.Titre,
                request.Description,
                request.PersonneImpliquee,
                http.User.GetUserId());

            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var id = await handler.Handle(command, ct);
            return Results.Created($"/api/events/{id}", new { id });
        })
        .RequireAuthorization(Policies.Events.Create);

        // GET /api/events/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            GetEventByIdQueryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetEventByIdQuery(id);
            var result = await handler.Handle(query, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        // GET /api/events?Page=1&PageSize=10&Status=draft
        group.MapGet("/", async (
            [AsParameters] GetEventsPagedQuery query,
            GetEventsPagedQueryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(query, ct);
            return Results.Ok(result);
        });

        return app;
    }
}