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
    public class AdminController(IUserRepository userRepository) : ControllerBase
    {
        [HttpGet("multipliers")]
        public async Task<IActionResult> GetCostMultipliers()
        {
            var multipliers = await userRepository.GetCostMultipliersAsync();
            return Ok(multipliers);
        }

        [HttpPost("multipliers")]
        public async Task<IActionResult> SaveCostMultipliers([FromBody] CostMultipliers multipliers)
        {
            if (multipliers == null)
            {
                return BadRequest("Invalid cost multipliers request.");
            }

            await userRepository.SaveCostMultipliersAsync(multipliers);
            return Ok(new { message = "Cost multipliers updated successfully" });
        }
    }
}
