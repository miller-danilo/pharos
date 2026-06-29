using Pharos.Core.Constants;
using Pharos.Core.Interfaces;
using Pharos.Core.Models;
using Pharos.Core.Utilities;
using System;
using System.Threading.Tasks;

namespace Pharos.Core.Services
{
    public class ShieldService(IAiService aiService, IScanRepository scanRepository, IUserRepository userRepository) : IShieldService
    {
        public async Task<JobAnalysisResult> AnalyzeScanAsync(string? text, byte[]? fileBytes, string? mimeType, string? fileName, string userId)
        {
            string hash = string.Empty;
            if (fileBytes != null)
            {
                hash = HashHelper.ComputeSha256(fileBytes);
            }
            else if (!string.IsNullOrEmpty(text))
            {
                hash = HashHelper.ComputeSha256(text);
            }

            var cachedScan = await scanRepository.GetScanByHashAsync(hash);
            if (cachedScan != null)
            {
                await scanRepository.SaveScanAsync(cachedScan);
                var cachedResult = System.Text.Json.JsonSerializer.Deserialize<JobAnalysisResult>(cachedScan.AnalysisJson);
                if (cachedResult != null)
                {
                    return cachedResult;
                }
            }

            var result = await aiService.AnalyzeJobAsync(text, fileBytes, mimeType);

            var scan = new Scan
            {
                Hash = hash,
                Title = text != null ? (text.Length > 100 ? text.Substring(0, 100) : text) : (fileName ?? "Uploaded file"),
                Company = CreditDefaults.DefaultCompany,
                RiskLevel = result.RiskLevel,
                RawText = text ?? string.Empty,
                AnalysisJson = System.Text.Json.JsonSerializer.Serialize(result),
                CreatedAt = DateTime.UtcNow
            };
            await scanRepository.SaveScanAsync(scan);

            var txn = new Transaction
            {
                UserId = userId,
                CreditsChanged = 0,
                Reason = FirestoreConstants.TransactionReasons.JobSafetyScan,
                CreatedAt = DateTime.UtcNow,
                Usage = new UsageTelemetry
                {
                    PromptTokens = result.PromptTokens,
                    CompletionTokens = result.CompletionTokens,
                    DbReads = TelemetryDefaults.DbOperations.ScanReads,
                    DbWrites = TelemetryDefaults.DbOperations.ScanWrites
                }
            };
            await userRepository.LogTransactionAsync(txn);

            return result;
        }
    }
}
