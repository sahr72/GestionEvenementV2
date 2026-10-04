using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using EventManagement.Domain.Entities;
using EventManagement.Api.Security.Requirements;
using EventManagement.Api.Security.Handlers;

namespace EventManagement.Api.Tests.Security;

public class EventResourceHandlerTests
{ /*
    Ce qu'on teste ici
    La matrice de décision : Admin = true, Propriétaire = true, Autre = false.
    Aucune dépendance HTTP, aucune base de données, aucun WebApplicationFactory.
    Le AuthorizationHandlerContext est la seule abstraction ASP.NET Core nécessaire, et elle est instanciable directement.
    Note : la ClaimsIdentity doit recevoir un authenticationType (ici "Test") et son RoleClaimType par défaut est bien 
    ClaimTypes.Role — c'est ce qui permet à IsInRole("Administrator") de fonctionner dans le test.
  */
    [Fact]
    public async Task HandleAsync_UserIsAdministrator_Succeeds()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Administrator")
        }, "TestAuth"));

        var evt = Evenement.Create("Conf", "Descrip", "personneImpl", Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        var requirement = new EvenementResourceRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement }, user, evt);

        var handler = new EvenementResourceAuthorizationHandler();

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserIsOwner_Succeeds()
    {
        var ownerId = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, ownerId.ToString())
        }, "TestAuth"));

        var evt = Evenement.Create("Conf", "Descrip", "personneImpl", ownerId, DateTime.UtcNow.AddDays(1) );
        var context = new AuthorizationHandlerContext(
            new[] { new EvenementResourceRequirement() }, user, evt);

        var handler = new EvenementResourceAuthorizationHandler();
        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task HandleAsync_UserIsNeitherOwnerNorAdmin_Fails()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }, "TestAuth"));

        var evt = Evenement.Create("Conf", "Descrip","personneImpl", Guid.NewGuid(),DateTime.UtcNow.AddDays(1));
        var context = new AuthorizationHandlerContext(
            new[] { new EvenementResourceRequirement() }, user, evt);

        var handler = new EvenementResourceAuthorizationHandler();
        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}