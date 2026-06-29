using Moq;
using Pharos.Core.Interfaces;
using Pharos.Core.Models;
using Pharos.Core.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Pharos.Core.Tests.Services
{
    public class ShieldServiceTests
    {
        private readonly Mock<IAiService> _aiServiceMock;
        private readonly Mock<IScanRepository> _scanRepoMock;
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly ShieldService _service;

        public ShieldServiceTests()
        {
            _aiServiceMock = new Mock<IAiService>();
            _scanRepoMock = new Mock<IScanRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _service = new ShieldService(_aiServiceMock.Object, _scanRepoMock.Object, _userRepoMock.Object);
        }

        [Fact]
        public async Task AnalyzeScanAsync_CacheHit_ReturnsCachedResultAndUpdatesCount()
        {
            // Arrange
            string text = "suspicious job posting text";
            string userId = "user123";
            var cachedResult = new JobAnalysisResult { RiskLevel = "RED", Summary = "Scam" };
            var cachedScan = new Scan
            {
                Hash = "516BB994ACC4F90AF55141EDFDB916B3E6BD92D5A70CC2298FD5D3DEB74CCC56", // Sha256 of text
                AnalysisJson = JsonSerializer.Serialize(cachedResult)
            };

            _scanRepoMock.Setup(r => r.GetScanByHashAsync(It.IsAny<string>()))
                .ReturnsAsync(cachedScan);

            // Act
            var result = await _service.AnalyzeScanAsync(text, null, null, null, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("RED", result.RiskLevel);
            _scanRepoMock.Verify(r => r.GetScanByHashAsync(cachedScan.Hash), Times.Once);
            _scanRepoMock.Verify(r => r.SaveScanAsync(cachedScan), Times.Once);
            _aiServiceMock.Verify(a => a.AnalyzeJobAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AnalyzeScanAsync_CacheMiss_CallsAiAndSavesScanAndLogsTransaction()
        {
            // Arrange
            string text = "legit developer role";
            string userId = "user123";
            var aiResult = new JobAnalysisResult
            {
                RiskLevel = "GREEN",
                Summary = "Clean",
                PromptTokens = 120,
                CompletionTokens = 80
            };

            _scanRepoMock.Setup(r => r.GetScanByHashAsync(It.IsAny<string>()))
                .ReturnsAsync((Scan?)null);

            _aiServiceMock.Setup(a => a.AnalyzeJobAsync(text, null, null))
                .ReturnsAsync(aiResult);

            // Act
            var result = await _service.AnalyzeScanAsync(text, null, null, null, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GREEN", result.RiskLevel);
            _scanRepoMock.Verify(r => r.SaveScanAsync(It.Is<Scan>(s => s.RiskLevel == "GREEN" && s.RawText == text)), Times.Once);
            _userRepoMock.Verify(r => r.LogTransactionAsync(It.Is<Transaction>(t => t.UserId == userId && t.CreditsChanged == 0 && t.Usage!.PromptTokens == 120)), Times.Once);
        }
    }
}
