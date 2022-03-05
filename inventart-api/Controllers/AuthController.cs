using Inventart.Authorization;
using Inventart.Config;
using Inventart.Models.ControllerInputs;
using Inventart.Repos;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using bCrypt = BCrypt.Net.BCrypt;

namespace Inventart.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _wenv;
        private readonly ConnectionStringProvider _csp;
        private readonly JwtService _jwt;
        private readonly AuthRepo _repo;
        private readonly EmailService _email;
        private readonly GlobalConfig _globalConfig;

        public AuthController(
            ILogger<AuthController> logger,
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider,
            JwtService jwtService,
            AuthRepo authRepo,
            EmailService emailService, 
            IOptions<GlobalConfig> globalConfig)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
            _jwt = jwtService;
            _repo = authRepo;
            _email = emailService;
            _globalConfig = globalConfig.Value;
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

        [Requires(Permission.ListPainting)]
        [HttpPost("tryauth")]
        public async Task<IActionResult> TryAuth()
        {
            return Ok("passed");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(AuthRegister input)
        {
            if (string.IsNullOrWhiteSpace(input.Email)) return BadRequest("empty.email");
            if (string.IsNullOrWhiteSpace(input.Password)) return BadRequest("empty.password");
            if (string.IsNullOrWhiteSpace(input.PasswordRepeat)) return BadRequest("empty.password");
            if (string.IsNullOrWhiteSpace(input.FirstName)) return BadRequest("empty.first.name");
            if (string.IsNullOrWhiteSpace(input.LastName)) return BadRequest("empty.last.name");

            // validate email and return BadRequest if it does not conform
            try
            {
                MailAddress addr = new MailAddress(input.Email);
            }
            catch
            {
                return BadRequest("invalid.email");
            }

            // validate password and return BadRequest if it does not conform
            if (input.Password != input.PasswordRepeat)
                return BadRequest("passwords.dont.match");

            if (input.Password.Length < 4)
                return BadRequest("password.too.small");

            // hash the password
            string passwordHash = bCrypt.HashPassword(input.Password);
            string defaultTenant = this.getUserDefaultTenant(input.Email);
            Guid? verificationGuid = null;
            try
            {
                verificationGuid = _repo.UserRegistration(input.Email, passwordHash, input.FirstName, input.LastName, defaultTenant);
            }
            catch (Exception x) //WIP: check sql exception for duplicate key 
            {
                //if (px.SqlState == "23505")
                //{
                //    return BadRequest("that email already exists");
                //}
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
            catch (Exception x)
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
                return BadRequest("email.not.found");
            //check if the passwords match
            bool verified = false;
            try
            {
                verified = bCrypt.Verify(input.Password, user.password_hash);
            } catch
            {
                verified = false;
            }
            if (!verified)
                return BadRequest("wrong.password");
            //check if the user has verified the email
            if (user.verified == false)
                return BadRequest("email.unverified");
            //create and return the jwt token
            var userToken = new UserToken(guid: user.guid);
            string token = _jwt.GenerateJwtToken(userToken);
            return Ok(token);
        }

        [HttpPost("user-info")]
        [Requires()]
        public async Task<IActionResult> UserInfo()
        {
            Guid userGuid = (Guid)HttpContext.Items["UserGuid"];

            dynamic userInfo = await _repo.UserInfo(userGuid);
            
            return Ok(userInfo);
        }

        [HttpPost("user-tenants")]
        [Requires()]
        public async Task<IActionResult> UserTenants()
        {
            Guid userGuid = (Guid)HttpContext.Items["UserGuid"];

            var userTenants = await _repo.UserTenants(userGuid);

            return Ok(userTenants);
        }

        [HttpPost("user-tenant")]
        [Requires()]
        public async Task<IActionResult> UserTenant(string tenant)
        {
            Guid userGuid = (Guid)HttpContext.Items["UserGuid"];

            var userTenant = await _repo.UserTenant(userGuid, tenant);

            return Ok(userTenant);
        }

        [HttpPost("password-reset-step1")]
        public async Task<IActionResult> PasswordResetStep1(string email)
        {
            Guid? resetGuid = null;
            try
            {
                resetGuid = _repo.PasswordResetStep1(email);
            }
            catch (Exception x)
            {
                throw;
            }

            if (!resetGuid.HasValue)
                return BadRequest();

            //send mail
            _email.SendPasswordResetLink(email, resetGuid.Value);

            return Ok();
        }

        [HttpPost("password-reset-step2a")]
        public async Task<IActionResult> PasswordResetStep2a(Guid password_reset_guid)
        {
            bool exists = false;
            try
            {
                exists = _repo.PasswordResetStep2a(password_reset_guid);
            }
            catch (Exception x)
            {
                throw;
            }

            if (exists)
                return Ok();

            return BadRequest();
        }

        [HttpPost("password-reset-step2b")]
        public async Task<IActionResult> PasswordResetStep2b(AuthPasswordReset input)
        {
            if (string.IsNullOrWhiteSpace(input.Password)) return BadRequest("empty.password");
            if (string.IsNullOrWhiteSpace(input.PasswordRepeat)) return BadRequest("empty.password");

            // validate password and return BadRequest if it does not conform
            if (input.Password != input.PasswordRepeat)
                return BadRequest("passwords.dont.match");

            if (input.Password.Length < 4)
                return BadRequest("password.too.small");

            // hash the password
            string passwordHash = bCrypt.HashPassword(input.Password);

            bool success = false;
            try
            {
                success = _repo.PasswordResetStep2b(input.PasswordResetGuid, passwordHash);
            }
            catch (Exception x)
            {
                throw;
            }

            if (!success)
                return BadRequest();

            return Ok();
        }

        private string getUserDefaultTenant(string email)
        {
            string emailSuffix = $"@{email.Split('@')[1]}";
            string defaultTenant = string.Empty;
            if (_globalConfig.DefaultTenantsEmail.ContainsKey(emailSuffix))
                return _globalConfig.DefaultTenantsEmail[emailSuffix];

            return defaultTenant;
        }
    }
}