using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pharos.Core.Interfaces;
using Pharos.Core.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // C# 12 Primary Constructor
    public class ShieldController(IAiService aiService, IScanRepository scanRepository) : ControllerBase
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
                    // Fallback to bad request
                }
            }

            if (string.IsNullOrEmpty(text) && file == null)
            {
                return BadRequest("Must provide job description text or file to scan.");
            }

            string hash = string.Empty;
            byte[]? fileBytes = null;
            string? mimeType = null;

            if (file != null)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                fileBytes = ms.ToArray();
                mimeType = file.ContentType;
                hash = ComputeHash(fileBytes);
            }
            else if (!string.IsNullOrEmpty(text))
            {
                hash = ComputeHash(text);
            }

            try
            {
                var cachedScan = await scanRepository.GetScanByHashAsync(hash);
                if (cachedScan != null)
                {
                    _ = scanRepository.SaveScanAsync(cachedScan);
                    var cachedResult = System.Text.Json.JsonSerializer.Deserialize<JobAnalysisResult>(cachedScan.AnalysisJson);
                    if (cachedResult != null)
                    {
                        return Ok(cachedResult);
                    }
                }

                var result = await aiService.AnalyzeJobAsync(text, fileBytes, mimeType);

                var scan = new Scan
                {
                    Hash = hash,
                    Title = text != null ? (text.Length > 100 ? text.Substring(0, 100) : text) : (file?.FileName ?? "Uploaded file"),
                    Company = "Unknown",
                    RiskLevel = result.RiskLevel,
                    RawText = text ?? string.Empty,
                    AnalysisJson = System.Text.Json.JsonSerializer.Serialize(result),
                    CreatedAt = DateTime.UtcNow
                };
                await scanRepository.SaveScanAsync(scan);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error analyzing job vacancy: {ex.Message}");
            }
        }

        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        private static string ComputeHash(byte[] input)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(input);
            return Convert.ToHexString(bytes);
        }

        public class ScanRequest
        {
            public string? Text { get; set; }
        }
    }
}
