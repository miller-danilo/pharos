using Microsoft.AspNetCore.Mvc;
using Pharos.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    
    public class PaymentController(IPaymentWebhookService webhookService) : ControllerBase
    {
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            string requestBody = await reader.ReadToEndAsync();

            Request.Headers.TryGetValue("X-Signature", out var signatureHeader);

            try
            {
                await webhookService.ProcessWebhookAsync(requestBody, signatureHeader);
                return Ok(new { message = "Webhook processed successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing the webhook.");
            }
        }
    }
}
