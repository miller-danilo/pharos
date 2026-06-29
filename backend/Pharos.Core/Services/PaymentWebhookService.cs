using Microsoft.Extensions.Configuration;
using Pharos.Core.Constants;
using Pharos.Core.Interfaces;
using Pharos.Core.Utilities;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pharos.Core.Services
{
    public class PaymentWebhookService : IPaymentWebhookService
    {
        private readonly ICreditRepository _creditRepo;
        private readonly string _webhookSecret;

        public PaymentWebhookService(ICreditRepository creditRepo, IConfiguration configuration)
        {
            _creditRepo = creditRepo;
            _webhookSecret = configuration["LemonSqueezy:WebhookSecret"] 
                             ?? Environment.GetEnvironmentVariable("LEMON_SQUEEZY_WEBHOOK_SECRET") 
                             ?? string.Empty;
        }

        public async Task ProcessWebhookAsync(string requestBody, string? signature)
        {
            if (!string.IsNullOrEmpty(_webhookSecret))
            {
                if (string.IsNullOrEmpty(signature))
                {
                    throw new ArgumentException("Missing X-Signature security header.");
                }

                string computedSignature = HashHelper.ComputeHmacSha256(requestBody, _webhookSecret);
                if (!string.Equals(computedSignature, signature, StringComparison.OrdinalIgnoreCase))
                {
                    throw new UnauthorizedAccessException("Invalid X-Signature security header.");
                }
            }

            using var doc = JsonDocument.Parse(requestBody);
            var root = doc.RootElement;

            string eventName = root.GetProperty("meta").GetProperty("event_name").GetString() ?? string.Empty;
            if (eventName != LemonSqueezyConstants.OrderCreatedEvent)
            {
                return;
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
                throw new ArgumentException("No userId found in custom_data.");
            }

            var data = root.GetProperty("data");
            string orderId = data.GetProperty("id").GetString() ?? Guid.NewGuid().ToString();
            
            int creditsQuantity = CreditDefaults.DefaultFallbackQuantity;

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
                if (subtotalProp.ValueKind == JsonValueKind.Number)
                {
                    cost = subtotalProp.GetDecimal() / 100m;
                }
            }
            else if (attributes.TryGetProperty("total", out var totalProp))
            {
                if (totalProp.ValueKind == JsonValueKind.Number)
                {
                    cost = totalProp.GetDecimal() / 100m;
                }
            }

            if (attributes.TryGetProperty("currency", out var currencyProp))
            {
                currency = currencyProp.GetString() ?? "USD";
            }

            await _creditRepo.AddCreditsFromPaymentAsync(userId, creditsQuantity, orderId, cost, currency);
        }
    }
}
