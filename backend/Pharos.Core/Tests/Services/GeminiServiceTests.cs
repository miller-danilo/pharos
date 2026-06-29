using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Pharos.Core.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pharos.Core.Tests.Services
{
    public class GeminiServiceTests
    {
        [Fact]
        public async Task AnalyzeJobAsync_ApiKeyMissing_ThrowsInvalidOperationException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Gemini:ApiKey", ""}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var httpClient = new HttpClient();
            var service = new GeminiService(httpClient, configuration);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AnalyzeJobAsync("Job description"));
        }

        [Fact]
        public async Task AnalyzeJobAsync_Success_ReturnsJobAnalysisResult()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Gemini:ApiKey", "fake_key"},
                {"Gemini:Model", "gemini-2.5-flash"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var mockResponseJson = @"{
                ""candidates"": [
                    {
                        ""content"": {
                            ""parts"": [
                                {
                                    ""text"": ""{\""riskLevel\"": \""GREEN\"", \""summary\"": \""Test verdict.\"", \""reasoning\"": \""Looks good\"", \""factors\"": []}""
                                }
                            ]
                        }
                    }
                ],
                ""usageMetadata"": {
                    ""promptTokenCount"": 100,
                    ""candidatesTokenCount"": 50,
                    ""totalTokenCount"": 150
                }
            }";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(mockResponseJson, System.Text.Encoding.UTF8, "application/json"),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new GeminiService(httpClient, configuration);

            // Act
            var result = await service.AnalyzeJobAsync("Job vacancy text");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GREEN", result.RiskLevel);
            Assert.Equal("Test verdict.", result.Summary);
        }

        [Fact]
        public async Task GenerateProposalAsync_Success_ReturnsLetter()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Gemini:ApiKey", "fake_key"},
                {"Gemini:Model", "gemini-2.5-flash"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var mockResponseJson = @"{
                ""candidates"": [
                    {
                        ""content"": {
                            ""parts"": [
                                {
                                    ""text"": ""Generated Cover Letter text""
                                }
                            ]
                        }
                    }
                ],
                ""usageMetadata"": {
                    ""promptTokenCount"": 200,
                    ""candidatesTokenCount"": 100,
                    ""totalTokenCount"": 300
                }
            }";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(mockResponseJson, System.Text.Encoding.UTF8, "application/json"),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new GeminiService(httpClient, configuration);

            // Act
            var result = await service.GenerateProposalAsync("CV text", "Job details");

            // Assert
            Assert.Equal("Generated Cover Letter text", result.ProposalText);
        }

        [Fact]
        public async Task AnalyzeJobAsync_HttpError_ThrowsHttpRequestException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Gemini:ApiKey", "fake_key"}
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                  StatusCode = HttpStatusCode.InternalServerError,
                  Content = new StringContent("API key invalid or model error")
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new GeminiService(httpClient, configuration);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => service.AnalyzeJobAsync("Job details"));
        }

        [Fact]
        public async Task GenerateProposalAsync_EmptyResponse_ThrowsInvalidOperationException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?> {
                {"Gemini:ApiKey", "fake_key"}
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

            var mockResponseJson = @"{
                ""candidates"": []
            }";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                  StatusCode = HttpStatusCode.OK,
                  Content = new StringContent(mockResponseJson, System.Text.Encoding.UTF8, "application/json")
               });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new GeminiService(httpClient, configuration);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateProposalAsync("CV text", "Job details"));
        }
    }
}
