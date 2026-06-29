using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharos.Core.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserController(IUserRepository userRepository) : ControllerBase
    {
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            string userId = User.GetUserId();
            string email = User.GetEmail();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await userRepository.GetOrCreateUserAsync(userId, email);
            return Ok(user);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            string userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var transactions = await userRepository.GetUserTransactionsAsync(userId);
            return Ok(transactions);
        }

        [HttpPost("cv")]
        public async Task<IActionResult> UpdateCv([FromBody] CvUpdateRequest request)
        {
            string userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(request.CvText))
            {
                return BadRequest("CV text cannot be empty.");
            }

            await userRepository.UpdateUserCvAsync(userId, request.CvText);
            return Ok(new { message = "CV updated successfully" });
        }

        public class CvUpdateRequest
        {
            public string CvText { get; set; } = string.Empty;
        }
    }
}
