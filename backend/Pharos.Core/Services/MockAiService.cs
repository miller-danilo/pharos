using Pharos.Core.Interfaces;
using Pharos.Core.Models;
using Pharos.Core.Constants;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Pharos.Core.Services
{
    [ExcludeFromCodeCoverage]
    public class MockAiService : IAiService
    {
        public Task<JobAnalysisResult> AnalyzeJobAsync(string? text, byte[]? fileBytes = null, string? mimeType = null)
        {
            var mockResult = new JobAnalysisResult
            {
                RiskLevel = "YELLOW",
                Summary = "Esta es una evaluación simulada para desarrollo local (Mock).",
                Reasoning = "Se ha detectado el uso del entorno de desarrollo local con el servicio de Inteligencia Artificial simulado.",
                Factors = new List<AnalysisFactor>
                {
                    new()
                    {
                        Name = "Servicio Mock Activo",
                        Description = "La aplicación está configurada para usar respuestas simuladas.",
                        IsRisk = false
                    },
                    new()
                    {
                        Name = "Validación de Entorno",
                        Description = "Advertencia de que no se están consumiendo créditos reales de la API de Gemini.",
                        IsRisk = true
                    }
                }
            };

            return Task.FromResult(mockResult);
        }

        public Task<ProposalResult> GenerateProposalAsync(string cvText, string jobText)
        {
            string mockProposal = @"# Carta de Presentación Simulada (Local Mock)

Estimado equipo de contratación,

Esta es una propuesta y carta de presentación simulada generada localmente por el servicio de desarrollo de Pharos.

## Detalles de la Simulación:
- **CV Recibido**: Contiene información simulada del candidato.
- **Detalles del Trabajo**: Contiene información simulada de la oferta.

Saludos cordiales,
El equipo de Pharos (Simulación)";

            var result = new ProposalResult
            {
                ProposalText = mockProposal,
                PromptTokens = CreditDefaults.MockPromptTokens,
                CompletionTokens = CreditDefaults.MockCompletionTokens
            };

            return Task.FromResult(result);
        }
    }
}
