using EventManagement.Application.Authorization;
using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EventManagement.Api.Security.Filters
{//Garde-fou : un filtre par ressource. Pas de ResourceAuthorizationFilter<T> générique, pas de ResourceResolver<T>,
 //pas de pipeline abstrait
    public sealed class CommentaireResourceAuthorizationFilter(
    ICommentaireAuthorizationReadService readService,
    IAuthorizationService authorization) : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var http = context.HttpContext;

            // ATTENTION : la clé de route est "commentId", pas "id".
            var id = (Guid)http.Request.RouteValues["commentId"]!;

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
}
