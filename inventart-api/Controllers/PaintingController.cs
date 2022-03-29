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
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Inventart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaintingController : ControllerBase
    {
        private readonly ILogger<PaintingController> _logger;
        private readonly IWebHostEnvironment _wenv;
        private readonly ConnectionStringProvider _csp;
        private readonly PaintingRepo _repo;

        public PaintingController(
            ILogger<PaintingController> logger,
            IWebHostEnvironment webHostEnvironment,
            ConnectionStringProvider connectionStringProvider,
            PaintingRepo paintingRepo)
        {
            _logger = logger;
            _wenv = webHostEnvironment;
            _csp = connectionStringProvider;
            _repo = paintingRepo;
        }

        [HttpGet("{tenant}/list-all")]
        [Requires(Permission.ListPainting)]
        public async Task<IActionResult> Get([FromRoute] string tenant)
        {
            List<dynamic> results = await _repo.ListAll(tenant);
            return Ok(results);
        }

        [HttpPost("upload")]
        [Requires(Permission.UploadFile)]
        public async Task<IActionResult> UploadFile([FromQuery] Guid painting, IFormFile file)
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
                    file_guid = await _repo.SaveFile(painting, file.FileName, fileBytes);
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
        [Requires(Permission.CreatePainting)]
        public async Task<IActionResult> Create([FromRoute] string tenant, [FromBody] PaintingDto painting)
        {
            if (string.IsNullOrWhiteSpace(painting.Name)) return BadRequest("empty.name");
            if (string.IsNullOrWhiteSpace(painting.Author)) return BadRequest("empty.author");

            Guid? guid;
            try
            {
                guid = await _repo.Create(tenant, painting);
            }
            catch (SqlException x)
            {
                if (x.Number == 2627)
                    return BadRequest("duplicated.name.author");
                return BadRequest("db.error");
            }
            catch (Exception x)
            {
                return BadRequest("generic");
            }
            return Ok(new { guid = guid });
        }

        [HttpGet("{tenant}/select/{guid}")]
        [Requires(Permission.ListPainting)]
        public async Task<IActionResult> SelectSingle([FromRoute] string tenant, [FromRoute] Guid guid)
        {
            dynamic painting = await _repo.SelectSingle(tenant, guid);
            return Ok(painting);
        }

        [HttpPost("{tenant}/update/{guid}")]
        [Requires(Permission.CreatePainting)]
        public async Task<IActionResult> Update([FromRoute] string tenant, [FromRoute] Guid guid, [FromBody] PaintingDto painting)
        {
            if (string.IsNullOrWhiteSpace(painting.Name)) return BadRequest("empty.name");
            if (string.IsNullOrWhiteSpace(painting.Author)) return BadRequest("empty.author");

            try
            {
                await _repo.Update(tenant, guid, painting);
            }
            catch (SqlException x)
            {
                if (x.Number == 2627)
                    return BadRequest("duplicated.name.author");
                return BadRequest("db.error");
            }
            catch (Exception x)
            {
                return BadRequest("generic");
            }
            return Ok();
        }
    }
}
