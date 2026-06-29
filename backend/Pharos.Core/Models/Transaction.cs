using Google.Cloud.Firestore;
using System;

using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Models
{
    [FirestoreData]
    [ExcludeFromCodeCoverage]
    public class UsageTelemetry
    {
        [FirestoreProperty("promptTokens")]
        public int PromptTokens { get; set; }

        [FirestoreProperty("completionTokens")]
        public int CompletionTokens { get; set; }

        [FirestoreProperty("dbReads")]
        public int DbReads { get; set; }

        [FirestoreProperty("dbWrites")]
        public int DbWrites { get; set; }
    }

    [FirestoreData]
    [ExcludeFromCodeCoverage]
    public class Transaction
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("userId")]
        public string UserId { get; set; } = string.Empty;

        [FirestoreProperty("creditsChanged")]
        public long CreditsChanged { get; set; }

        [FirestoreProperty("reason")]
        public string Reason { get; set; } = string.Empty;

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty("currency")]
        public string Currency { get; set; } = string.Empty;

        [FirestoreProperty("usage")]
        public UsageTelemetry? Usage { get; set; }
    }
}
