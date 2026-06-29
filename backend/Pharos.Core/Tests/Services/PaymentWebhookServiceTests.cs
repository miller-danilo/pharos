using Microsoft.Extensions.Configuration;
using Moq;
using Pharos.Core.Interfaces;
using Pharos.Core.Services;
using Pharos.Core.Utilities;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Pharos.Core.Tests.Services
{
    public class PaymentWebhookServiceTests
    {
        private readonly Mock<ICreditRepository> _creditRepoMock;
        private readonly Mock<IConfiguration> _configMock;

        public PaymentWebhookServiceTests()
        {
            _creditRepoMock = new Mock<ICreditRepository>();
            _configMock = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task ProcessWebhookAsync_MissingSignature_ThrowsArgumentException()
        {
            // Arrange
            _configMock.Setup(c => c["LemonSqueezy:WebhookSecret"]).Returns("secret");
            var service = new PaymentWebhookService(_creditRepoMock.Object, _configMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.ProcessWebhookAsync("{}", null));
        }

        [Fact]
        public async Task ProcessWebhookAsync_InvalidSignature_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _configMock.Setup(c => c["LemonSqueezy:WebhookSecret"]).Returns("secret");
            var service = new PaymentWebhookService(_creditRepoMock.Object, _configMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.ProcessWebhookAsync("{}", "invalid-signature"));
        }

        [Fact]
        public async Task ProcessWebhookAsync_IgnoredEvent_ReturnsWithoutAction()
        {
            // Arrange
            _configMock.Setup(c => c["LemonSqueezy:WebhookSecret"]).Returns((string?)null);
            var service = new PaymentWebhookService(_creditRepoMock.Object, _configMock.Object);
            string body = JsonSerializer.Serialize(new
            {
                meta = new { event_name = "subscription_created" }
            });

            // Act
            await service.ProcessWebhookAsync(body, null);

            // Assert
            _creditRepoMock.Verify(c => c.AddCreditsFromPaymentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessWebhookAsync_ValidOrderCreated_ProcessesSuccessfully()
        {
            // Arrange
            _configMock.Setup(c => c["LemonSqueezy:WebhookSecret"]).Returns("secret");
            var service = new PaymentWebhookService(_creditRepoMock.Object, _configMock.Object);
            
            string body = JsonSerializer.Serialize(new
            {
                meta = new
                {
                    event_name = "order_created",
                    custom_data = new { user_id = "user123" }
                },
                data = new
                {
                    id = "order-999",
                    attributes = new
                    {
                        first_order_item = new { quantity = 15 },
                        total = 5990, // $59.90
                        currency = "USD"
                    }
                }
            });

            string signature = HashHelper.ComputeHmacSha256(body, "secret");

            // Act
            await service.ProcessWebhookAsync(body, signature);

            // Assert
            _creditRepoMock.Verify(c => c.AddCreditsFromPaymentAsync("user123", 15, "order-999", 59.90m, "USD"), Times.Once);
        }
    }
}
