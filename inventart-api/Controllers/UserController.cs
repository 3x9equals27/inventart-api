using Inventart.Authorization;
using Inventart.Models.ControllerInputs;
using Inventart.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Inventart.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserRepo _repo;

        public UserController(
            ILogger<AuthController> logger,
            UserRepo userRepo)
        {
            _repo = userRepo;
        }

        [HttpPost("edit-self")]
        [Requires(Permission.EditSelf)]
        public async Task<IActionResult> EditSelf(UserInfoEdit input)
        {
            //WIP validate user input
            Guid userGuid = (Guid)HttpContext.Items["UserGuid"];

            await _repo.SetUserInfo(userGuid, input.FirstName, input.LastName, input.DefaultTenant, input.DefaultLanguage);

            return Ok();
        }

        [HttpGet("{tenant}/roles")]
        [Requires(Permission.ListUserRole)]
        public async Task<IActionResult> ListAllUsersRole([FromRoute] string tenant)
        {
            List<dynamic> results = await _repo.ListAllUsersRole(tenant);
            return Ok(results);
        }

        [HttpPost("{tenant}/roleChange/{guid}/{role}")]
        [Requires(Permission.EditRoles)]
        public async Task<IActionResult> RoleChange([FromRoute] string tenant, [FromRoute] Guid guid, [FromRoute] string role)
        {
            await _repo.EditUserRole(guid, tenant, role);
            return Ok();
        }
    }
}