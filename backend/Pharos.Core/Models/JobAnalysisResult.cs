using System.Collections.Generic;

using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Models
{
    [ExcludeFromCodeCoverage]
    public class JobAnalysisResult
    {
        public string RiskLevel { get; set; } = "YELLOW"; // GREEN, YELLOW, RED
        public string Summary { get; set; } = string.Empty;
        public string Reasoning { get; set; } = string.Empty;
        public List<AnalysisFactor> Factors { get; set; } = new();
    }

    [ExcludeFromCodeCoverage]
    public class AnalysisFactor
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRisk { get; set; }
    }
}
