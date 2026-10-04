using EventManagement.Api.Security.Requirements;
using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EventManagement.Api.Security.Handlers
{
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
}
