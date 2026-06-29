using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Pharos.Core.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pharos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    
    public class PaymentController(IUserRepository userRepository, IConfiguration configuration) : ControllerBase
    {
        private readonly string _webhookSecret = configuration["LemonSqueezy:WebhookSecret"] 
                                                 ?? Environment.GetEnvironmentVariable("LEMON_SQUEEZY_WEBHOOK_SECRET") 
                                                 ?? string.Empty;

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            string requestBody = await reader.ReadToEndAsync();

            if (!string.IsNullOrEmpty(_webhookSecret))
            {
                if (!Request.Headers.TryGetValue("X-Signature", out var signatureHeader))
                {
                    return BadRequest("Missing X-Signature security header.");
                }

                string computedSignature = ComputeHmacSha256(requestBody, _webhookSecret);
                if (!string.Equals(computedSignature, signatureHeader, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized("Invalid X-Signature security header.");
                }
            }

            try
            {
                using var doc = JsonDocument.Parse(requestBody);
                var root = doc.RootElement;

                string eventName = root.GetProperty("meta").GetProperty("event_name").GetString() ?? string.Empty;
                if (eventName != "order_created")
                {
                    return Ok(new { message = $"Event '{eventName}' ignored." });
                }

                var meta = root.GetProperty("meta");
                string userId = string.Empty;
                if (meta.TryGetProperty("custom_data", out var customData))
                {
                    if (customData.TryGetProperty("user_id", out var userIdProp))
                    {
                        userId = userIdProp.GetString() ?? string.Empty;
                    }
                    else if (customData.TryGetProperty("userId", out var userIdProp2))
                    {
                        userId = userIdProp2.GetString() ?? string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("No userId found in custom_data.");
                }

                var data = root.GetProperty("data");
                string orderId = data.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
                
                int creditsQuantity = 10; //TODO: Fallback default

                var attributes = data.GetProperty("attributes");
                if (attributes.TryGetProperty("first_order_item", out var firstOrderItem))
                {
                    if (firstOrderItem.TryGetProperty("quantity", out var quantityProp))
                    {
                        creditsQuantity = quantityProp.GetInt32();
                    }
                }

                decimal cost = 0;
                string currency = "USD";

                if (attributes.TryGetProperty("subtotal", out var subtotalProp))
                {
                    if (subtotalProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        cost = subtotalProp.GetDecimal() / 100m;
                    }
                }
                else if (attributes.TryGetProperty("total", out var totalProp))
                {
                    if (totalProp.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        cost = totalProp.GetDecimal() / 100m;
                    }
                }

                if (attributes.TryGetProperty("currency", out var currencyProp))
                {
                    currency = currencyProp.GetString() ?? "USD";
                }

                await userRepository.AddCreditsFromPaymentAsync(userId, creditsQuantity, orderId, cost, currency);

                return Ok(new { message = $"Credits successfully added. User: {userId}, Quantity: {creditsQuantity}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing webhook: {ex.Message}");
            }
        }

        private static string ComputeHmacSha256(string message, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
