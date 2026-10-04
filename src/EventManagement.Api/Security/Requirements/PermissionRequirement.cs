using Microsoft.AspNetCore.Authorization;

namespace EventManagement.Api.Security.Requirements
{
    public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
    {
        public string Permission { get; } = permission;
    }
}
