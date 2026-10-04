using EventManagement.Application.Authorization;

namespace EventManagement.Api.Security.TokenIssuance
{
    /// <summary>
    /// *Lecture du mapping : Administrator et Organizer portent les mêmes permissions. 
    /// Ce qui les distingue n'est pas le jeton de permission, mais le bypass ressource IsInRole("Administrator") (voir §1.3). 
    /// Les rôles ne sont donc pas trois ensembles de permissions disjoints : 
    /// ce sont des personas dont la différence se joue à deux endroits précis (permissions + bypass ressource).
    /// <
    /// *//summary>
    public static class RolePermissions
    {
        // Gestion complète des événements + commentaires.
        private static readonly string[] EventManager =
        [
            Permissions.Events.Read,  Permissions.Events.Create, Permissions.Events.Update,
        Permissions.Events.Confirm, Permissions.Events.Cancel, Permissions.Events.Close,
        Permissions.Comments.Read, Permissions.Comments.Create,
        Permissions.Comments.Update, Permissions.Comments.Delete
        ];

        // Consultation des événements + commentaires (et gestion de SES commentaires).
        private static readonly string[] Participant =
        [
            Permissions.Events.Read,
        Permissions.Comments.Read, Permissions.Comments.Create,
        Permissions.Comments.Update, Permissions.Comments.Delete
        ];

        public static IReadOnlyList<string> For(string role) => role switch
        {
            Roles.Administrator => EventManager,
            Roles.Organizer => EventManager,
            Roles.Participant => Participant,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Rôle inconnu.")
        };
    }
}
