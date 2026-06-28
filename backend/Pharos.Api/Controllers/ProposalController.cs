using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharos.Core.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    // C# 12 Primary Constructor
    public class ProposalController(IProposalService proposalService) : ControllerBase
    {
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] ProposalRequest request)
        {
            string userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(request.JobText))
            {
                return BadRequest("Job text must be provided.");
            }

            try
            {
                string proposal = await proposalService.GenerateProposalWithCreditControlAsync(userId, request.CvText, request.JobText);
                return Ok(new { proposal });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Insufficient credits.")
            {
                return StatusCode(402, "Insufficient credits. Please purchase more credits.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating proposal: {ex.Message}");
            }
        }

        public class ProposalRequest
        {
            public string JobText { get; set; } = string.Empty;
            public string CvText { get; set; } = string.Empty;
        }
    }
}
