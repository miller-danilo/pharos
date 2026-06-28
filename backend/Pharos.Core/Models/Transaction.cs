using Google.Cloud.Firestore;
using System;

using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Models
{
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
    }
}
