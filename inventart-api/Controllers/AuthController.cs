using Dapper;
using Inventart.Services.Singleton;
using Inventart.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Inventart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _wenv;
        private readonly ConnectionStringProvider _csp;
        private readonly JwtService _jwt;

        public AuthController(
            ILogger<AuthController> logger,
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider,
            JwtService jwtService)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
            _jwt = jwtService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test()
        {
            //var x = Request.Headers["Authorization"];
            var userToken = new UserToken(guid: Guid.Parse("076f5d62-6fda-44c0-b3bd-95ee0369ff8f"), role: "role:guest");
            string token = _jwt.GenerateJwtToken(userToken);
            UserToken result = _jwt.ValidateJwtToken(token);

            return Ok(new {UserToken = result, TokenString = token});
        }

     
    }

}
