using Google.Cloud.Firestore;
using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Models
{
    [FirestoreData]
    [ExcludeFromCodeCoverage]
    public class CostMultipliers
    {
        [FirestoreProperty("geminiInputTokenRate")]
        public decimal GeminiInputTokenRate { get; set; } = 0.000000075m;

        [FirestoreProperty("geminiOutputTokenRate")]
        public decimal GeminiOutputTokenRate { get; set; } = 0.0000003m;

        [FirestoreProperty("firestoreReadRate")]
        public decimal FirestoreReadRate { get; set; } = 0.0000006m;

        [FirestoreProperty("firestoreWriteRate")]
        public decimal FirestoreWriteRate { get; set; } = 0.0000018m;
    }
}
