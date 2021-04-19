using Inventart.Authorization;
using Inventart.Models.ControllerInputs;
using Inventart.Repos;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Threading.Tasks;
using bCrypt = BCrypt.Net.BCrypt;

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
        private readonly AuthRepo _repo;
        private readonly EmailService _email;

        public AuthController(
            ILogger<AuthController> logger,
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider,
            JwtService jwtService,
            AuthRepo authRepo,
            EmailService emailService)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
            _jwt = jwtService;
            _repo = authRepo;
            _email = emailService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test()
        {
            //var x = Request.Headers["Authorization"];
            var userToken = new UserToken(guid: Guid.Parse("95b3ffa8-4904-4860-92e3-400be5a5b08a"));
            string token = _jwt.GenerateJwtToken(userToken);
            UserToken result = _jwt.ValidateJwtToken(token);
            result = _jwt.ValidateJwtToken("Bearer " + token);

            return Ok(new { UserToken = result, TokenString = $"Bearer {token}" });
        }

        [Requires(Permission.ListDiagnostic)]
        [HttpPost("tryauth")]
        public async Task<IActionResult> TryAuth()
        {
            return Ok("passed");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthRegister input)
        {
            // validate email and return BadRequest if it does not conform

            // validate password and return BadRequest if it does not conform

            // has the password
            string passwordHash = bCrypt.HashPassword(input.Password);
            Guid? verificationGuid = null;
            try
            {
                verificationGuid = _repo.UserRegistration(input.Email, passwordHash);
            }
            catch (PostgresException px)
            {
                if (px.SqlState == "23505")
                {
                    return BadRequest("that email already exists");
                }
                throw;
            }
            _email.SendVerificationLink(input.Email, verificationGuid.Value);
            return Ok();
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(Guid verificationGuid)
        {
            // validate email and return BadRequest if it does not conform

            bool success = false;
            try
            {
                success = _repo.UserVerification(verificationGuid);
            }
            catch (PostgresException px)
            {
                throw;
            }

            if (!success)
                return BadRequest();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthLogin input)
        {
            var user = await _repo.UserForLogin(input.Email);
            //check if a user with that email exists
            if (user == null)
                return BadRequest("Wrong email or password.");
            //check if the passwords match
            bool verified = bCrypt.Verify(input.Password, user.password_hash);
            if (!verified)
                return BadRequest("Wrong email or password.");
            //chec if the user has verified the email
            if (user.verified == false)
                return BadRequest("Email not verified.");
            //create and return the jwt token
            var userToken = new UserToken(guid: user.guid);
            string token = _jwt.GenerateJwtToken(userToken);
            return Ok(token);
        }
    }
}