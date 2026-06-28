namespace Pharos.Core.Constants
{
    public static class FirestoreConstants
    {
        public static class Collections
        {
            public const string Users = "users";
            public const string Scans = "scans";
            public const string Transactions = "transactions";
        }

        public static class TransactionReasons
        {
            public const string WelcomeBalance = "welcome_balance";
            public const string ProposalGeneration = "proposal_generation";
            public const string ProposalGenerationRefund = "proposal_generation_refund";
        }
    }
}
