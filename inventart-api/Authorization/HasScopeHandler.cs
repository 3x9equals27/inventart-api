using inventart_api.Authorization;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
    {
        // check if the user has verified his email
        /*
        var email_verified = context.User.FindFirst(c => c.Type == $"{requirement.Namespace}verified" && c.Issuer == requirement.Issuer)?.Value.Trim()??"false";
        if(email_verified != "true")
            return Task.CompletedTask;
        */
        // get the user role
        var role = (context.User.FindFirst(c => c.Type == "permissions" && c.Issuer == requirement.Issuer)?.Value??string.Empty).Split(' ')[0];
        
        // Succeed if the permissions array contains the required permission
        if (PermissionManager.Check(requirement.Permission, role))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
