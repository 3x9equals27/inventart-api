using Microsoft.AspNetCore.Authorization;
using System;

public class HasScopeRequirement : IAuthorizationRequirement
{
    public string Issuer { get; }
    public string Permission { get; }
    public string Namespace { get; }

    public HasScopeRequirement(string permission, string issuer, string jwt_namespace)
    {
        Permission = permission ?? throw new ArgumentNullException(nameof(permission));
        Issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        Namespace = jwt_namespace ?? throw new ArgumentNullException(nameof(jwt_namespace));
    }
}
