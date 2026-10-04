using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Application.Authorization
{
    //noms des policies sur ressource (usage exclusif avec AuthorizeAsync)
    public static class ResourceOperations
    {
        // Une policy par agrégat : la règle Proprietaire||Admin est identique
        // pour update/confirm/cancel/close. On ne duplique pas.
        public static class Events
        {
            public const string Manage = "Event.Manage.Resource";
        }

        // La règle Auteur||Admin est identique pour update/delete.
        public static class Comments
        {
            public const string Manage = "Comment.Manage.Resource";
        }
    }
}
