using Inventart.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.Extensions.DependencyInjection;
using Inventart.Services.Singleton;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresAttribute : Attribute, IAuthorizationFilter 
{
    private readonly string requiredPermission; 
    public RequiresAttribute(string permission)
    {
        requiredPermission = permission;
    }


    public void OnAuthorization(AuthorizationFilterContext context)
    {
        JwtService _jwt = context.HttpContext.RequestServices.GetService<JwtService>();
        //auth attribute Requires(Permission)
        // 1 - get token and tenant from headers
        // 2 - validate token and get user guid
        // 3 - use user guid and tenant to get user role on that tennats
        // 4 - check if role has the permissions specified on [Authorize(permisson)]

        string test = requiredPermission; //WIP remeber to use this permission later om

        string[] authorizations = context.HttpContext.Request.Headers["Authorization"].ToArray();
        if (authorizations.Length != 1) //return 401 if more than 1 Auth Header is found
        {
            context.Result = new JsonResult(new { message = "YOU SHALL NOT PASS" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }
        //spliting Bearer and token
        string[] authorization = authorizations[0].Split(" ");
        //make sure they are 2: 1 Bearer and 1 token
        if (authorization.Length != 2)
        {
            context.Result = new JsonResult(new { message = "YOU SHALL NOT PASS" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }
        if (authorization[0] != "Bearer")
        {
            context.Result = new JsonResult(new { message = "YOU SHALL NOT PASS" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }
        UserToken userToken = _jwt.ValidateJwtToken(authorization[1]);
        if (userToken == null)
        {
            context.Result = new JsonResult(new { message = "YOU SHALL NOT PASS" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }

        string tenantId = context.HttpContext.Request.Headers["x-tenant-id"];
        


        /*
        var account = (Account)context.HttpContext.Items["Account"];
        if (account == null)
        {
            // not logged in
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
        */

        context.Result = new JsonResult(new { message = "YOU SHALL NOT PASS" }) { StatusCode = StatusCodes.Status401Unauthorized };
    }
}