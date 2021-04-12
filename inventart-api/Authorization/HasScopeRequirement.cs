using Microsoft.AspNetCore.Authorization;
using System;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Issuer { get; }
    public string Permission { get; }

    public HasScopeRequirement(string permission, string issuer)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
    }
}
