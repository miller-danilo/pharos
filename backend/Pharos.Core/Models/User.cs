using Google.Cloud.Firestore;
using System;

using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Models
{
    [FirestoreData]
    [ExcludeFromCodeCoverage]
    public class User
    {
        [FirestoreProperty("id")]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty("email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("credits")]
        public long Credits { get; set; }

        [FirestoreProperty("cvText")]
        public string CvText { get; set; } = string.Empty;

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty("creditLockedAt")]
        public DateTime? CreditLockedAt { get; set; }
    }
}
