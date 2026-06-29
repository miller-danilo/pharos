using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharos.Core.Interfaces;
using Pharos.Core.Models.Requests;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]    
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
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while generating the proposal.");
            }
        }
    }
}
