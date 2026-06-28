using Google.Cloud.Firestore;
using System;

using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Models
{
    [FirestoreData]
    [ExcludeFromCodeCoverage]
    public class Scan
    {
        [FirestoreProperty("hash")]
        public string Hash { get; set; } = string.Empty;

        [FirestoreProperty("title")]
        public string Title { get; set; } = string.Empty;

        [FirestoreProperty("company")]
        public string Company { get; set; } = string.Empty;

        [FirestoreProperty("riskLevel")]
        public string RiskLevel { get; set; } = string.Empty; // GREEN, YELLOW, RED

        [FirestoreProperty("rawText")]
        public string RawText { get; set; } = string.Empty;

        [FirestoreProperty("analysisJson")]
        public string AnalysisJson { get; set; } = string.Empty;

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty("count")]
        public long Count { get; set; }
    }
}
