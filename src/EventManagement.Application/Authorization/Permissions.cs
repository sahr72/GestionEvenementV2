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
            public const string Read = "Event.Read";
            public const string Create = "Event.Create";
            public const string Update = "Event.Update";
            public const string Confirm = "Event.Confirm";
            public const string Cancel = "Event.Cancel";
            public const string Close = "Event.Close";
        }

        public static class Comments
        {
            public const string Read = "Comment.Read";
            public const string Create = "Comment.Create";
            public const string Update = "Comment.Update";
            public const string Delete = "Comment.Delete";
        }
    }
}
