using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class ProposalResult
    {
        public string ProposalText { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
    }
}
