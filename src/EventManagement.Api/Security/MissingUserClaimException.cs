namespace EventManagement.Api.Security
{
    // Exception dédiée : le jeton est authentifiable mais ne porte pas d'identité exploitable.
    // Elle vit dans Api/Security : c'est une préoccupation d'IDENTITÉ, pas de domaine ni d'application.
    /*
     * Pourquoi dans Api/Security et non dans Application ou Domain ? 
     * L'Application ne connaît ni les claims ni les jetons (§4.5) ; 
     * le Domaine encore moins (§0). Placer ce type ailleurs ferait fuiter une notion d'identité HTTP dans les couches 
     * basses — exactement ce que le labo interdit. Api/Security et Api/Middleware appartiennent 
     * au même assembly : le GlobalExceptionHandler peut donc la référencer sans créer de dépendance nouvelle.
     * */
    public sealed class MissingUserClaimException(string claimType)
        : Exception($"La claim d'identité '{claimType}' est absente ou invalide dans le jeton.")
    {
        public string ClaimType { get; } = claimType;
    }
}
