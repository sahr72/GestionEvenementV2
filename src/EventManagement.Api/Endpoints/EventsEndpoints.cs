// src/EventManagement.Api/Endpoints/EventsEndpoints.cs

namespace EventManagement.Api.Endpoints;

using EventManagement.Application.Commands;
using EventManagement.Application.Commands.CreateEvent;
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
public static class EventsEndpoints
{
    public static IEndpointRouteBuilder MapEventEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events")
            .WithTags("Events")
            .WithOpenApi();

        // POST /api/events
        group.MapPost("/", async (
            CreateEventRequest request,
            IValidator<CreateEventCommand> validator,
            CreateEventCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new CreateEventCommand(
                request.Titre,
                request.Description,
                request.PersonneImpliquee);

            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var id = await handler.Handle(command, ct);
            return Results.Created($"/api/events/{id}", new { id });
        });

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