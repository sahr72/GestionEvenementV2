using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Application.Authorization
{
    //vocabulaire d'autorisation
    public static class Permissions
    {
        public static class Events
        {
            public const string Read = "Events.Read";
            public const string Create = "Events.Create";
            public const string Update = "Events.Update";
            public const string Confirm = "Events.Confirm";
            public const string Cancel = "Events.Cancel";
            public const string Close = "Events.Close";
        }

        public static class Comments
        {
            public const string Read = "Comments.Read";
            public const string Create = "Comments.Create";
            public const string Update = "Comments.Update";
            public const string Delete = "Comments.Delete";
        }
    }
}
