﻿using inventart_api.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
    {
        // If user does not have the permission claim, get out of here
        if (!context.User.HasClaim(c => c.Type == "permissions" && c.Issuer == requirement.Issuer))
            return Task.CompletedTask;

        // Split the permissions string into an array
        var permissions = context.User.FindFirst(c => c.Type == "permissions" && c.Issuer == requirement.Issuer).Value.Split(' ');
        var role = permissions[0];
        // Succeed if the permissions array contains the required permission
        if (PermissionManager.Check(requirement.Permission, role))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
