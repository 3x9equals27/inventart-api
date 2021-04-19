using Dapper;
using Inventart.Authorization;
using Inventart.Config;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Hosting;
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
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly IWebHostEnvironment _wenv;
        private readonly ConnectionStringProvider _csp;

        public FileController(
            ILogger<FileController> logger,
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
        }

        [Requires(Permission.ListDiagnostic)]
        [HttpGet("link/{fileGuid}")]
        public async Task<IActionResult> GetLinkForFile(Guid fileGuid)
        {
            if (Guid.Empty == fileGuid) return BadRequest();

            string folderPath = Path.Combine(_wenv.WebRootPath, GlobalConfig.WwwRootModelFolder, $"{fileGuid}");

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath);
                if (files.Length == 1)
                {
                    return Ok(LinkToFile(fileGuid, Path.GetFileName(files[0])));
                }
                else
                {
                    Directory.Delete(folderPath, true);
                }
            }

            //WIP: get this in its own service and use a stored proc
            List<dynamic> results = new List<dynamic>();
            var sql = "SELECT name, bytes FROM file WHERE guid = @guid";

            using (var connection = new NpgsqlConnection(_csp.ConnectionString))
            {
                results = (await connection.QueryAsync(sql, new { guid = fileGuid })).ToList();
            }

            //return null when this is a function that has to return a object of type { name, bytes} and add this logic with a "is null" instead after the function returns
            if (results.Count != 1) return NotFound();

            byte[] fileBytes = results[0].bytes;
            string name = results[0].name;
            //

            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, name);
            FileIO.WriteAllBytes(filePath, fileBytes);

            return Ok(LinkToFile(fileGuid, name));
        }

        private string LinkToFile(Guid fileGuid, string fileName)
        {
            return $"{Request.Scheme}://{Request.Host.Value}/{GlobalConfig.WwwRootModelFolder}/{fileGuid}/{fileName}";
        }
    }
}