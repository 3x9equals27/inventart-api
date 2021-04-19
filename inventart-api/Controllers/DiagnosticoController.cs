using Dapper;
using Inventart.Authorization;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        [Requires(Permission.ListDiagnostic)]
        public async Task<IActionResult> Get()
        {
            var x = Request.Headers["Authorization"];

            List<dynamic> results = new List<dynamic>();
            var sql = "SELECT * FROM fn_list_diagnostico()";
            //
            using (var connection = new NpgsqlConnection(_csp.ConnectionString))
            {
                results = (await connection.QueryAsync(sql)).ToList();
            }
            return Ok(results);
        }

        [HttpPost("upload")]
        [Requires(Permission.UploadFile)]
        public async Task<IActionResult> OnPostUploadAsync([FromQuery] Guid diagnostico, IFormFile file)
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
                    DynamicParameters sp_params = new DynamicParameters(new { i_guid = diagnostico, i_name = file.FileName, i_bytes = fileBytes });
                    sp_params.Add("@o_file_guid", value: null, DbType.Guid, direction: ParameterDirection.InputOutput);
                    var sql = "CALL sp_upload_file_diagnostico(@i_guid, @i_name, @i_bytes, @o_file_guid)";
                    //
                    using (var connection = new NpgsqlConnection(_csp.ConnectionString))
                    {
                        connection.Execute(sql, sp_params);
                        file_guid = sp_params.Get<Guid>("o_file_guid");
                        // save to wwwroot if upload to database worked, future optimization after the non optimized process has been tested
                        // when a file is requested for stream we will check the disk and only download form database if teh files are not already available
                        //FileIO.WriteAllBytes(Path.Combine(_wenv.WebRootPath, "nxz", $"{file_guid}.nxz"), fileBytes);
                    }
                }
            }

            // Process uploaded files
            // Don't rely on or trust the FileName property without validation.

            return Ok(new { count = 1, size, file_guid });
        }
    }
}