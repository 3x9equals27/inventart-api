using Dapper;
using Inventart.Authorization;
using Inventart.Models.ControllerInputs;
using Inventart.Repos;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Inventart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticController : ControllerBase
    {
        private readonly ILogger<DiagnosticController> _logger;
        private readonly IWebHostEnvironment _wenv;
        private readonly ConnectionStringProvider _csp;
        private readonly DiagRepo _repo;

        public DiagnosticController(
            ILogger<DiagnosticController> logger,
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider,
            DiagRepo diagRepo)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
            _repo = diagRepo;
        }

        [HttpGet("{tenant}/list-all")]
        [Requires(Permission.ListDiagnostic)]
        public async Task<IActionResult> Get([FromRoute] string tenant)
        {
            List<dynamic> results = await _repo.ListAllDiagnostic(tenant);
            return Ok(results);
        }

        [HttpPost("upload")]
        [Requires(Permission.UploadFile)]
        public async Task<IActionResult> UploadFile([FromQuery] Guid diagnostico, IFormFile file)
        {
            Guid? file_guid = null;
            long size = file.Length;

            if (file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    byte[] fileBytes = ms.ToArray();
                    // save to database
                    file_guid = await _repo.SaveFile(diagnostico, file.FileName, fileBytes);
                    // save to wwwroot if upload to database worked, future optimization after the non optimized process has been tested
                    // when a file is requested for stream we will check the disk and only download form database if teh files are not already available
                    //FileIO.WriteAllBytes(Path.Combine(_wenv.WebRootPath, "nxz", $"{file_guid}.nxz"), fileBytes);
                    
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = 1, size, file_guid });
        }

        [HttpPost("{tenant}/create")]
        [Requires(Permission.CreateDiagnostic)]
        public async Task<IActionResult> Create([FromRoute] string tenant, [FromBody] DiagnosticCreate diagnostic)
        {
            Guid? guid;
            try
            {
                guid = await _repo.DiagnosticCreate(tenant, diagnostic);
            } catch (Exception x)
            {
                //WIP: catch SQLException and check the error number and set distinct translatable error message for each case
                //return BadRequest(new { errorMessage123 = "WIP: set (t) error messages here, server:msg" });
                return BadRequest("generic error creating new record" );
            }
            return Ok(new { guid = guid });
        }
    }
}
