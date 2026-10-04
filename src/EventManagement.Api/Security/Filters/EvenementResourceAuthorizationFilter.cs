using EventManagement.Application.Authorization;
using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EventManagement.Api.Security.Filters
{
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
}
