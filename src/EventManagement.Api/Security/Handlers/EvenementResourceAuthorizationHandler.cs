using EventManagement.Api.Security.Requirements;
using EventManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EventManagement.Api.Security.Handlers
{
    public sealed class EvenementResourceAuthorizationHandler
     : AuthorizationHandler<EvenementResourceRequirement, EvenementAuthorizationInfo>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            EvenementResourceRequirement requirement,
            EvenementAuthorizationInfo resource)
        {
            // Admin passe. Sinon, propriétaire uniquement.
            // AUCUNE référence au statut : c'est le domaine qui décide.
            /*  if (context.User.IsInRole("Administrator")
                  || context.User.GetUserId() == resource.ProprietaireId)
              {
                  context.Succeed(requirement);
              }
            */
            if (context.User.IsAdministrator()
                  || context.User.GetUserId() == resource.ProprietaireId)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
