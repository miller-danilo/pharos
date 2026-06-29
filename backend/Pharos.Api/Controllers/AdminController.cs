using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharos.Core.Interfaces;
using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AdminController(IConfigRepository configRepository) : ControllerBase
    {
        [HttpGet("multipliers")]
        public async Task<IActionResult> GetCostMultipliers()
        {
            try
            {
                var multipliers = await configRepository.GetCostMultipliersAsync();
                return Ok(multipliers);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving cost multipliers.");
            }
        }

        [HttpPost("multipliers")]
        public async Task<IActionResult> SaveCostMultipliers([FromBody] CostMultipliers multipliers)
        {
            if (multipliers == null)
            {
                return BadRequest("Invalid cost multipliers request.");
            }

            try
            {
                await configRepository.SaveCostMultipliersAsync(multipliers);
                return Ok(new { message = "Cost multipliers updated successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while saving cost multipliers.");
            }
        }
    }
}
