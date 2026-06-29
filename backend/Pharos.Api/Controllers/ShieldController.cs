using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pharos.Core.Interfaces;
using Pharos.Core.Models;
using Pharos.Core.Models.Requests;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShieldController(IShieldService shieldService) : ControllerBase
    {
        [HttpPost("analyze")]
        [EnableRateLimiting("PublicScanPolicy")]
        public async Task<IActionResult> Analyze([FromForm] string? text, IFormFile? file)
        {
            if (string.IsNullOrEmpty(text) && file == null)
            {
                try
                {
                    var jsonRequest = await HttpContext.Request.ReadFromJsonAsync<ScanRequest>();
                    text = jsonRequest?.Text;
                }
                catch
                {
                    //TODO: Fallback to bad request
                }
            }

            if (string.IsNullOrEmpty(text) && file == null)
            {
                return BadRequest("Must provide job description text or file to scan.");
            }

            byte[]? fileBytes = null;
            string? mimeType = null;
            string? fileName = null;

            if (file != null)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
                mimeType = file.ContentType;
                fileName = file.FileName;
            }

            try
            {
                string userId = User.GetUserId();
                var result = await shieldService.AnalyzeScanAsync(text, fileBytes, mimeType, fileName, userId);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while analyzing the job vacancy.");
            }
        }
    }
}
