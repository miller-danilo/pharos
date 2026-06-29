using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IShieldService
    {
        Task<JobAnalysisResult> AnalyzeScanAsync(string? text, byte[]? fileBytes, string? mimeType, string? fileName, string userId);
    }
}
