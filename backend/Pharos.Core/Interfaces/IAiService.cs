using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IAiService
    {
        Task<JobAnalysisResult> AnalyzeJobAsync(string? text, byte[]? fileBytes = null, string? mimeType = null);
        Task<string> GenerateProposalAsync(string cvText, string jobText);
    }
}
