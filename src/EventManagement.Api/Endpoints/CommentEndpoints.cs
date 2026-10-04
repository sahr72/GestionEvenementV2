// src/EventManagement.Api/Endpoints/CommentsEndpoints.cs

namespace EventManagement.Api.Endpoints;

using EventManagement.Api.Security;
using EventManagement.Api.Security.Filters;
using EventManagement.Application.Authorization;
using EventManagement.Application.Commands.AddComment;
using EventManagement.Application.Commands.DeleteComment;
using EventManagement.Application.Commands.UpdateComment;
using EventManagement.Application.DTOs;
using EventManagement.Application.Queries.GetComments;
using FluentValidation;

public static class CommentEndpoints
{
    public static IEndpointRouteBuilder MapCommentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/events/{eventId:guid}/comments")
            .WithTags("Comments")
            .WithOpenApi();

        // AddComment (identité en entrée, autorisation sur l'événement): POST /api/events/{eventId}/comments
        /*Note : AddComment ne passe pas par le filtre de ressource. La capacité à commenter est contrôlée par 
         * la permission globale Comments.Create ; l'autorité finale sur l'état de l'événement 
         * reste Evenement.AjouterCommentaire() (? 409 si l'événement est annulé/clôturé).
         * */
        group.MapPost("/", async (
        Guid eventId,
        AddCommentRequest request,
        IValidator<AddCommentCommand> validator,
        AddCommentCommandHandler handler,
        HttpContext http,
        CancellationToken ct) =>
        {
            var command = new AddCommentCommand(eventId, request.Description, http.User.GetUserId());

            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            var id = await handler.Handle(command, ct);
            return Results.Created($"/api/events/{eventId}/comments/{id}", new { id });
        })
        .RequireAuthorization(Policies.Comments.Create);

        //  PUT /api/events/{eventId}/comments/{commentId} UpdateComment (autorisation sur le commentaire)
        group.MapPut("/{commentId:guid}", async (
        Guid eventId, Guid commentId, UpdateCommentRequest request,
        IValidator<UpdateCommentCommand> validator,
        UpdateCommentCommandHandler handler, CancellationToken ct) =>
        {
            var command = new UpdateCommentCommand(eventId, commentId, request.Description);
            var validation = await validator.ValidateAsync(command, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            await handler.Handle(command, ct);
            return Results.NoContent();
        })
        .RequireAuthorization(Policies.Comments.Update)
        .AddEndpointFilter<CommentaireResourceAuthorizationFilter>();   // 404 / 403 (Auteur || Admin)

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