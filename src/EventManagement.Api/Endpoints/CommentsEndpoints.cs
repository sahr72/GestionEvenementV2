// src/EventManagement.Api/Endpoints/CommentsEndpoints.cs

namespace EventManagement.Api.Endpoints;

using EventManagement.Application.Commands.AddComment;
using EventManagement.Application.Commands.DeleteComment;
using EventManagement.Application.Commands.UpdateComment;
using EventManagement.Application.DTOs;
using EventManagement.Application.Queries.GetComments;
using FluentValidation;

public static class CommentsEndpoints
{
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events/{eventId:guid}/comments")
            .WithTags("Comments")
            .WithOpenApi();

        // POST /api/events/{eventId}/comments
        group.MapPost("/", async (
            Guid eventId,
            AddCommentRequest request,
            IValidator<AddCommentCommand> validator,
            AddCommentCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new AddCommentCommand(eventId, request.Description);

            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            var id = await handler.Handle(command, ct);
            return Results.Created(
                $"/api/events/{eventId}/comments/{id}",
                new { id });
        });

        // GET /api/events/{eventId}/comments
        group.MapGet("/", async (
            Guid eventId,
            GetCommentsQueryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetCommentsQuery(eventId);
            var result = await handler.Handle(query, ct);
            return Results.Ok(result);
        });

        // PUT /api/events/{eventId}/comments/{commentId}
        group.MapPut("/{commentId:guid}", async (
            Guid eventId,
            Guid commentId,
            UpdateCommentRequest request,
            IValidator<UpdateCommentCommand> validator,
            UpdateCommentCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new UpdateCommentCommand(eventId, commentId, request.Description);

            var validationResult = await validator.ValidateAsync(command, ct);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(validationResult.ToDictionary());
            }

            await handler.Handle(command, ct);
            return Results.NoContent();
        });

        // DELETE /api/events/{eventId}/comments/{commentId}
        group.MapDelete("/{commentId:guid}", async (
            Guid eventId,
            Guid commentId,
            DeleteCommentCommandHandler handler,
            CancellationToken ct) =>
        {
            var command = new DeleteCommentCommand(eventId, commentId);
            await handler.Handle(command, ct);
            return Results.NoContent();
        });

        return app;
    }
}