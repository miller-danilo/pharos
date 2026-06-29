using Google.Cloud.Firestore;
using Google.Api.Gax;
using Microsoft.Extensions.Configuration;
using Pharos.Core.Models;
using Pharos.Core.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Pharos.Core.Interfaces;

using System.Diagnostics.CodeAnalysis;

namespace Pharos.Core.Services
{
    [ExcludeFromCodeCoverage]
    public class FirestoreService : IUserRepository, ICreditRepository, IConfigRepository, IScanRepository
    {
        private readonly FirestoreDb _db;

        public FirestoreService(IConfiguration configuration)
        {
            string? projectId = configuration["Firestore:ProjectId"] ?? Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT");
            if (string.IsNullOrEmpty(projectId))
            {
                projectId = "pharos-ai-demo";
            }
            
            string? emulatorHost = configuration["Firestore:EmulatorHost"] ?? Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST");
            if (!string.IsNullOrEmpty(emulatorHost))
            {
                Environment.SetEnvironmentVariable("FIRESTORE_EMULATOR_HOST", emulatorHost);
            }

            var builder = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                EmulatorDetection = EmulatorDetection.EmulatorOrProduction
            };

            string? credentialsJson = configuration["Google:CredentialsJson"] ?? Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_JSON");
            if (!string.IsNullOrEmpty(credentialsJson))
            {
                builder.GoogleCredential = Google.Apis.Auth.OAuth2.GoogleCredential.FromJson(credentialsJson);
            }

            _db = builder.Build();
        }

        public async Task<User> GetOrCreateUserAsync(string userId, string email)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            
            User user = await _db.RunTransactionAsync<User>(async (Google.Cloud.Firestore.Transaction dbTransaction) =>
            {
                DocumentSnapshot snapshot = await dbTransaction.GetSnapshotAsync(userRef);
                if (snapshot.Exists)
                {
                    return snapshot.ConvertTo<User>();
                }
                
                var newUser = new User
                {
                    Id = userId,
                    Email = email,
                    Credits = CreditDefaults.WelcomeBalance,
                    CvText = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                
                dbTransaction.Create(userRef, newUser);
                
                DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document();
                var txn = new Pharos.Core.Models.Transaction
                {
                    Id = txnRef.Id,
                    UserId = userId,
                    CreditsChanged = CreditDefaults.WelcomeBalance,
                    Reason = FirestoreConstants.TransactionReasons.WelcomeBalance,
                    CreatedAt = DateTime.UtcNow,
                    Usage = new UsageTelemetry
                    {
                        PromptTokens = 0,
                        CompletionTokens = 0,
                        DbReads = 0,
                        DbWrites = TelemetryDefaults.DbOperations.WelcomeBalanceWrites
                    }
                };
                dbTransaction.Create(txnRef, txn);
                
                return newUser;
            });
            
            return user;
        }

        public async Task<long> GetUserCreditsAsync(string userId)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var user = snapshot.ConvertTo<User>();
                return user.Credits;
            }
            return 0;
        }

        public async Task<string> LockCreditAsync(string userId)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            string transactionId = string.Empty;
            
            await _db.RunTransactionAsync(async (Google.Cloud.Firestore.Transaction dbTransaction) =>
            {
                DocumentSnapshot snapshot = await dbTransaction.GetSnapshotAsync(userRef);
                if (!snapshot.Exists)
                {
                    throw new KeyNotFoundException($"User {userId} not found.");
                }
                
                var user = snapshot.ConvertTo<User>();
                
                // Concurrency lock check (expires in 5 minutes)
                if (user.CreditLockedAt != null && DateTime.UtcNow - user.CreditLockedAt.Value < TimeSpan.FromMinutes(CreditDefaults.LockTimeoutMinutes))
                {
                    throw new InvalidOperationException("An AI generation is already in progress.");
                }
                
                if (user.CreditLockedAt != null)
                {
                    // Lock is expired. Refresh timestamp.
                    dbTransaction.Update(userRef, "creditLockedAt", DateTime.UtcNow);
                    
                    DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document();
                    transactionId = txnRef.Id;
                    var txn = new Pharos.Core.Models.Transaction
                    {
                        Id = txnRef.Id,
                        UserId = userId,
                        CreditsChanged = 0,
                        Reason = FirestoreConstants.TransactionReasons.ProposalGeneration,
                        CreatedAt = DateTime.UtcNow
                    };
                    dbTransaction.Create(txnRef, txn);
                }
                else
                {
                    // Fresh lock. Check credits.
                    if (user.Credits <= 0)
                    {
                        throw new InvalidOperationException("Insufficient credits.");
                    }
                    
                    dbTransaction.Update(userRef, "credits", user.Credits - 1);
                    dbTransaction.Update(userRef, "creditLockedAt", DateTime.UtcNow);
                    
                    DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document();
                    transactionId = txnRef.Id;
                    var txn = new Pharos.Core.Models.Transaction
                    {
                        Id = txnRef.Id,
                        UserId = userId,
                        CreditsChanged = -1,
                        Reason = FirestoreConstants.TransactionReasons.ProposalGeneration,
                        CreatedAt = DateTime.UtcNow
                    };
                    dbTransaction.Create(txnRef, txn);
                }
            });

            return transactionId;
        }

        public async Task ConfirmCreditDeductionAsync(string userId, string transactionId, int promptTokens, int completionTokens)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            await userRef.UpdateAsync("creditLockedAt", null);

            if (!string.IsNullOrEmpty(transactionId))
            {
                DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document(transactionId);
                var usage = new UsageTelemetry
                {
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    DbReads = TelemetryDefaults.DbOperations.ProposalReads,
                    DbWrites = TelemetryDefaults.DbOperations.ProposalWrites
                };
                await txnRef.UpdateAsync("usage", usage);
            }
        }

        public async Task ReleaseCreditLockAsync(string userId)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            
            await _db.RunTransactionAsync(async (Google.Cloud.Firestore.Transaction dbTransaction) =>
            {
                DocumentSnapshot snapshot = await dbTransaction.GetSnapshotAsync(userRef);
                if (!snapshot.Exists)
                {
                    return;
                }
                
                var user = snapshot.ConvertTo<User>();
                if (user.CreditLockedAt != null)
                {
                    dbTransaction.Update(userRef, "credits", user.Credits + 1);
                    dbTransaction.Update(userRef, "creditLockedAt", null);
                    
                    DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document();
                    var txn = new Pharos.Core.Models.Transaction
                    {
                        Id = txnRef.Id,
                        UserId = userId,
                        CreditsChanged = 1,
                        Reason = FirestoreConstants.TransactionReasons.ProposalGenerationRefund,
                        CreatedAt = DateTime.UtcNow,
                        Usage = new UsageTelemetry
                        {
                            PromptTokens = 0,
                            CompletionTokens = 0,
                            DbReads = TelemetryDefaults.DbOperations.ProposalReads,
                            DbWrites = TelemetryDefaults.DbOperations.ProposalWrites
                        }
                    };
                    dbTransaction.Create(txnRef, txn);
                }
            });
        }

        public async Task AddCreditsFromPaymentAsync(string userId, int amount, string paymentId, decimal cost, string currency)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            
            await _db.RunTransactionAsync(async (Google.Cloud.Firestore.Transaction dbTransaction) =>
            {
                DocumentSnapshot snapshot = await dbTransaction.GetSnapshotAsync(userRef);
                long currentCredits = 0;
                if (snapshot.Exists)
                {
                    var user = snapshot.ConvertTo<User>();
                    currentCredits = user.Credits;
                    dbTransaction.Update(userRef, "credits", currentCredits + amount);
                }
                else
                {
                    var newUser = new User
                    {
                        Id = userId,
                        Email = string.Empty,
                        Credits = amount,
                        CvText = string.Empty,
                        CreatedAt = DateTime.UtcNow
                    };
                    dbTransaction.Create(userRef, newUser);
                }
                
                DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document();
                var txn = new Pharos.Core.Models.Transaction
                {
                    Id = txnRef.Id,
                    UserId = userId,
                    CreditsChanged = amount,
                    Reason = $"{FirestoreConstants.TransactionReasons.PaymentPurchasePrefix}{paymentId}",
                    CreatedAt = DateTime.UtcNow,
                    Currency = currency,
                    Usage = new UsageTelemetry
                    {
                        PromptTokens = 0,
                        CompletionTokens = 0,
                        DbReads = TelemetryDefaults.DbOperations.PaymentReads,
                        DbWrites = TelemetryDefaults.DbOperations.PaymentWrites
                    }
                };
                dbTransaction.Create(txnRef, txn);
            });
        }

        public async Task UpdateUserCvAsync(string userId, string cvText)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            await userRef.UpdateAsync("cvText", cvText);
        }

        public async Task<string> GetUserCvAsync(string userId)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            DocumentSnapshot snapshot = await userRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var user = snapshot.ConvertTo<User>();
                return user.CvText;
            }
            return string.Empty;
        }

        public async Task<Scan?> GetScanByHashAsync(string hash)
        {
            DocumentReference scanRef = _db.Collection(FirestoreConstants.Collections.Scans).Document(hash);
            DocumentSnapshot snapshot = await scanRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Scan>();
            }
            return null;
        }

        public async Task SaveScanAsync(Scan scan)
        {
            DocumentReference scanRef = _db.Collection(FirestoreConstants.Collections.Scans).Document(scan.Hash);
            DocumentSnapshot snapshot = await scanRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                var existing = snapshot.ConvertTo<Scan>();
                await scanRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "count", existing.Count + 1 },
                    { "createdAt", DateTime.UtcNow }
                });
            }
            else
            {
                scan.Count = 1;
                scan.CreatedAt = DateTime.UtcNow;
                await scanRef.SetAsync(scan);
            }
        }

        public async Task<List<Pharos.Core.Models.Transaction>> GetUserTransactionsAsync(string userId)
        {
            Query query = _db.Collection(FirestoreConstants.Collections.Transactions)
                             .WhereEqualTo("userId", userId)
                             .OrderByDescending("createdAt");
            
            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            var list = new List<Pharos.Core.Models.Transaction>();
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    list.Add(doc.ConvertTo<Pharos.Core.Models.Transaction>());
                }
            }
            return list;
        }

        public async Task LogTransactionAsync(Pharos.Core.Models.Transaction transaction)
        {
            DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document();
            transaction.Id = txnRef.Id;
            await txnRef.SetAsync(transaction);
        }

        public async Task<CostMultipliers> GetCostMultipliersAsync()
        {
            DocumentReference docRef = _db.Collection(FirestoreConstants.Collections.Config).Document(FirestoreConstants.Documents.CostMultipliers);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<CostMultipliers>();
            }
            return new CostMultipliers();
        }

        public async Task SaveCostMultipliersAsync(CostMultipliers multipliers)
        {
            DocumentReference docRef = _db.Collection(FirestoreConstants.Collections.Config).Document(FirestoreConstants.Documents.CostMultipliers);
            await docRef.SetAsync(multipliers);
        }
    }
}
