using Dapper;
using Inventart.Authorization;
using Inventart.Config;
using Inventart.Models.RepoOutputs;
using Inventart.Repos;
using Inventart.Services.Singleton;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
        private readonly FileRepo _repo;

        public FileController(
            ILogger<FileController> logger,
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider,
            FileRepo fileRepo)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
            _repo = fileRepo;
        }

        [Requires(Permission.ListPainting)]
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

            RepoFile file = await _repo.GetFile(fileGuid);
            if (file is null) return NotFound();
            //

            Directory.CreateDirectory(folderPath);
            string filePath = Path.Combine(folderPath, file.FileName);
            FileIO.WriteAllBytes(filePath, file.FileBytes);

            return Ok(LinkToFile(fileGuid, file.FileName));
        }

        private string LinkToFile(Guid fileGuid, string fileName)
        {
            return $"{Request.Scheme}://{Request.Host.Value}/{GlobalConfig.WwwRootModelFolder}/{fileGuid}/{fileName}";
        }
    }
}