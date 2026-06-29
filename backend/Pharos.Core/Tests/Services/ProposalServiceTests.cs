using Moq;
using Pharos.Core.Interfaces;
using Pharos.Core.Models;
using Pharos.Core.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Pharos.Core.Tests.Services
{
    public class ProposalServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IAiService> _aiServiceMock;
        private readonly ProposalService _service;

        public ProposalServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _aiServiceMock = new Mock<IAiService>();
            _service = new ProposalService(_userRepoMock.Object, _aiServiceMock.Object);
        }

        [Fact]
        public async Task GenerateProposalWithCreditControlAsync_NoCvInRequestOrDb_ThrowsArgumentException()
        {
            // Arrange
            _userRepoMock.Setup(r => r.GetUserCvAsync("user123"))
                .ReturnsAsync(string.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GenerateProposalWithCreditControlAsync("user123", "", "Software Engineer Job"));
        }

        [Fact]
        public async Task GenerateProposalWithCreditControlAsync_Success_OrchestratesCorrectTransactionSequence()
        {
            // Arrange
            string cv = "John Doe C# Developer";
            string job = "Lead C# Dev";
            string generatedProposal = "Dear Lead Developer...";

            _userRepoMock.Setup(r => r.GetUserCvAsync("user123"))
                .ReturnsAsync(cv);

            _userRepoMock.Setup(r => r.LockCreditAsync("user123"))
                .ReturnsAsync("mock-tx-123");

            var mockProposalResult = new ProposalResult
            {
                ProposalText = generatedProposal,
                PromptTokens = 100,
                CompletionTokens = 50
            };

            _aiServiceMock.Setup(a => a.GenerateProposalAsync(cv, job))
                .ReturnsAsync(mockProposalResult);

            // Act
            var result = await _service.GenerateProposalWithCreditControlAsync("user123", "", job);

            // Assert
            Assert.Equal(generatedProposal, result);
            _userRepoMock.Verify(r => r.LockCreditAsync("user123"), Times.Once);
            _aiServiceMock.Verify(a => a.GenerateProposalAsync(cv, job), Times.Once);
            _userRepoMock.Verify(r => r.ConfirmCreditDeductionAsync("user123", "mock-tx-123", 100, 50), Times.Once);
            _userRepoMock.Verify(r => r.ReleaseCreditLockAsync("user123"), Times.Never);
        }

        [Fact]
        public async Task GenerateProposalWithCreditControlAsync_AiThrowsException_ReleasesCreditLockAndPropagatesException()
        {
            // Arrange
            string cv = "John Doe C# Developer";
            string job = "Lead C# Dev";

            _userRepoMock.Setup(r => r.GetUserCvAsync("user123"))
                .ReturnsAsync(cv);

            _userRepoMock.Setup(r => r.LockCreditAsync("user123"))
                .ReturnsAsync("mock-tx-123");

            _aiServiceMock.Setup(a => a.GenerateProposalAsync(cv, job))
                .ThrowsAsync(new Exception("Gemini API connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.GenerateProposalWithCreditControlAsync("user123", "", job));

            _userRepoMock.Verify(r => r.LockCreditAsync("user123"), Times.Once);
            _userRepoMock.Verify(r => r.ConfirmCreditDeductionAsync("user123", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _userRepoMock.Verify(r => r.ReleaseCreditLockAsync("user123"), Times.Once);
        }

        [Fact]
        public async Task GenerateProposalWithCreditControlAsync_WithCvInRequest_SkipsRepositoryLookup()
        {
            // Arrange
            string cv = "John Doe C# Developer";
            string job = "Lead C# Dev";
            string generatedProposal = "Dear Lead Developer...";

            _userRepoMock.Setup(r => r.LockCreditAsync("user123"))
                .ReturnsAsync("mock-tx-123");

            var mockProposalResult = new ProposalResult
            {
                ProposalText = generatedProposal,
                PromptTokens = 100,
                CompletionTokens = 50
            };

            _aiServiceMock.Setup(a => a.GenerateProposalAsync(cv, job))
                .ReturnsAsync(mockProposalResult);

            // Act
            var result = await _service.GenerateProposalWithCreditControlAsync("user123", cv, job);

            // Assert
            Assert.Equal(generatedProposal, result);
            _userRepoMock.Verify(r => r.GetUserCvAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GenerateProposalWithCreditControlAsync_ReleaseLockThrowsException_SuppressesReleaseErrorAndPropagatesOriginalError()
        {
            // Arrange
            string cv = "John Doe C# Developer";
            string job = "Lead C# Dev";

            _userRepoMock.Setup(r => r.GetUserCvAsync("user123"))
                .ReturnsAsync(cv);

            _aiServiceMock.Setup(a => a.GenerateProposalAsync(cv, job))
                .ThrowsAsync(new Exception("Original LLM error"));

            _userRepoMock.Setup(r => r.ReleaseCreditLockAsync("user123"))
                .ThrowsAsync(new Exception("Lock release db connection error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.GenerateProposalWithCreditControlAsync("user123", "", job));

            Assert.Equal("Original LLM error", ex.Message);
        }
    }
}
