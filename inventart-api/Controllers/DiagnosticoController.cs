using Dapper;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileIO = System.IO.File;

namespace Inventart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiagnosticoController : ControllerBase
    {
        private readonly ILogger<DiagnosticoController> _logger;
        private readonly IWebHostEnvironment _wenv;
        private readonly ConnectionStringProvider _csp;

        public DiagnosticoController(
            ILogger<DiagnosticoController> logger, 
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
        }

        [HttpGet("list")]
        public IEnumerable<string> Get()
        {
            return new List<string>() { "1", "2"};
        }

        [HttpPost("upload")]
        public async Task<IActionResult> OnPostUploadAsync([FromQuery]Guid diagnostico, IFormFile file)
        {
            long size = file.Length;

            if (file.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    byte[] fileBytes = ms.ToArray();
                    var x = 1;
                    // save to database
                    using (var connection = new NpgsqlConnection(_csp.ConnectionString))
                    {
                        var sql = "UPDATE diagnostico SET nxz_file = (@file) WHERE guid = (@guid)";
                        if (1 == connection.Execute(sql, new { file = fileBytes, guid = diagnostico }))
                        {
                            // save to wwwroot if upload to database worked
                            FileIO.WriteAllBytes(Path.Combine(_wenv.WebRootPath, "nxz", "db.nxz"), fileBytes);
                        }
                        //var cenas = connection.Query("SELECT * FROM test");
                    }
                }
            }
            
            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = 1, size });
        }

    }
}
