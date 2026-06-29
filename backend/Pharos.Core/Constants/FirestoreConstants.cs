namespace Pharos.Core.Constants
{
    public static class FirestoreConstants
    {
        public static class Collections
        {
            public const string Users = "users";
            public const string Scans = "scans";
            public const string Transactions = "transactions";
            public const string Config = "config";
        }

        public static class Documents
        {
            public const string CostMultipliers = "cost_multipliers";
        }

        public static class TransactionReasons
        {
            public const string WelcomeBalance = "welcome_balance";
            public const string ProposalGeneration = "proposal_generation";
            public const string ProposalGenerationRefund = "proposal_generation_refund";
            public const string JobSafetyScan = "job_safety_scan";
            public const string PaymentPurchasePrefix = "payment_purchase:";
        }
    }
}

