namespace EventManagement.Api.Security.TokenIssuance
{
    public static class Roles
    {
        public const string Administrator = "Administrator";
        public const string Organizer = "Organizer";
        public const string Participant = "Participant";

        public static readonly IReadOnlySet<string> All =
            new HashSet<string> { Administrator, Organizer, Participant };
    }
}
