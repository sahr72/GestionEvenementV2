using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Application.Authorization
{
//endpoint-level(valeurs dérivées, aucune divergence possible)
    public static class Policies
    {
        public static class Events
        {
            public const string Read = Permissions.Events.Read;
            public const string Create = Permissions.Events.Create;
            public const string Update = Permissions.Events.Update;
            public const string Confirm = Permissions.Events.Confirm;
            public const string Cancel = Permissions.Events.Cancel;
            public const string Close = Permissions.Events.Close;
        }

        public static class Comments
        {
            public const string Read = Permissions.Comments.Read;
            public const string Create = Permissions.Comments.Create;
            public const string Update = Permissions.Comments.Update;
            public const string Delete = Permissions.Comments.Delete;
        }
    }
}
