using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pharos.Core.Models;

using Pharos.Core.Interfaces;

namespace Pharos.Core.Services
{
    public class GeminiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
            // Default model is gemini-2.5-flash as it is the standard flash model
            _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        }

        public async Task<JobAnalysisResult> AnalyzeJobAsync(string? text, byte[]? fileBytes = null, string? mimeType = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured.");
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            var parts = new List<object>();

            if (fileBytes != null && !string.IsNullOrEmpty(mimeType))
            {
                parts.Add(new
                {
                    inlineData = new
                    {
                        mimeType = mimeType,
                        data = Convert.ToBase64String(fileBytes)
                    }
                });
            }

            if (!string.IsNullOrEmpty(text))
            {
                parts.Add(new { text = text });
            }

            string systemPrompt = @"You are Pharos, an expert job vacancy risk analyzer. Your job is to analyze the vacancy content (text, PDF, or screenshot) and classify its fraud/risk level.
You must categorize the job into one of these risk levels:
- GREEN: Safe job. Highly legitimate, no red flags.
- YELLOW: Warning. Some suspicious elements, lacking details, or slightly unusual requirements.
- RED: Danger/Fraud. Highly likely a scam. Promotes fraudulent tasks, asks for upfront fees, requests moving to Telegram or Whatsapp for quick tasks and immediate payouts, or has other high-risk indicators.

You MUST return a JSON object adhering to this schema:
{
  ""riskLevel"": ""GREEN"" | ""YELLOW"" | ""RED"",
  ""summary"": ""A brief 1-2 sentence summary of the verdict."",
  ""reasoning"": ""Detailed reasoning behind the risk classification."",
  ""factors"": [
    {
      ""name"": ""Name of the factor (e.g., 'Telegram Redirect', 'Professional email address')"",
      ""description"": ""Detailed explanation of this factor in the vacancy."",
      ""isRisk"": true | false
    }
  ]
}
Make sure all text fields are in Spanish, as Pharos primarily serves LatAm candidates. Return ONLY the JSON object, with no markdown formatting tags.";

            parts.Add(new { text = systemPrompt });

            var requestPayload = new
            {
                contents = new[]
                {
                    new { parts = parts.ToArray() }
                },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    temperature = 0.2
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestPayload);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
            }

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            string? jsonText = (geminiResponse?.Candidates != null && geminiResponse.Candidates.Length > 0)
                ? geminiResponse.Candidates[0]?.Content?.Parts?[0]?.Text
                : null;

            if (string.IsNullOrEmpty(jsonText))
            {
                throw new InvalidOperationException("Failed to get response from Gemini API.");
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<JobAnalysisResult>(jsonText, options)
                   ?? throw new InvalidOperationException("Failed to deserialize job analysis result.");
        }

        public async Task<string> GenerateProposalAsync(string cvText, string jobText)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured.");
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

            string prompt = $@"You are an elite career coach and resume writer.
Generate an optimized, highly persuasive cover letter and technical pitch in Spanish, aligning the candidate's professional profile (CV) with the requirements of the job vacancy.

Candidate CV/Profile:
{cvText}

Job Vacancy Details:
{jobText}

Guidelines:
1. Address the core needs of the job description.
2. Align candidate's key achievements and skills.
3. Keep the tone professional, persuasive, and authentic.
4. Output the result in clean Markdown format. Do not add any introductory or concluding chat filler, return ONLY the generated cover letter/pitch.";

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7
                }
            };

            var response = await _httpClient.PostAsJsonAsync(url, requestPayload);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
            }

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            string? letter = (geminiResponse?.Candidates != null && geminiResponse.Candidates.Length > 0)
                ? geminiResponse.Candidates[0]?.Content?.Parts?[0]?.Text
                : null;

            if (string.IsNullOrEmpty(letter))
            {
                throw new InvalidOperationException("Failed to generate cover letter from Gemini API.");
            }

            return letter.Trim();
        }

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public Candidate[]? Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
        }

        private class Content
        {
            [JsonPropertyName("parts")]
            public Part[]? Parts { get; set; }
        }

        private class Part
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
