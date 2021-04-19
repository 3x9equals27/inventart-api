using Inventart.Authorization;
using Inventart.Repos;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequiresAttribute : Attribute, IAuthorizationFilter
{
    //WIP: remove DEFAULT_TENANT_CODE after tenacy logic is added to the frontend
    private const string DEFAULT_TENANT_CODE = "FBAUL";

    private readonly string requiredPermission;

    public RequiresAttribute(string permission)
    {
        requiredPermission = permission;
    }

    // 1 - get token and tenant from headers
    // 2 - validate token and get user guid
    // 3 - use user guid and tenant to get user role on that tennats
    // 4 - check if role has the permissions specified on [Authorize(permisson)]

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        JwtService _jwt = context.HttpContext.RequestServices.GetService<JwtService>();
        AuthRepo _repo = context.HttpContext.RequestServices.GetService<AuthRepo>();
        //auth attribute Requires(Permission)

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

        Guid userGuid = userToken.guid;
        string tenant = context.HttpContext.GetRouteData()?.Values["tenant"]?.ToString() ?? DEFAULT_TENANT_CODE;
        string role = Task.Run(() => _repo.RoleOfUserTenant(userGuid, tenant)).Result; //can't do awaits here

        if (string.IsNullOrEmpty(role))
        {
            context.Result = new JsonResult(new { message = "YOU SHALL NOT PASS" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }
        if (false == PermissionManager.Check(this.requiredPermission, role))
        {
            context.Result = new JsonResult(new { message = "YOU SHALL NOT PASS" }) { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }
    }
}