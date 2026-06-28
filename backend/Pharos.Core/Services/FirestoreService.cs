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
    public class FirestoreService : IUserRepository, IScanRepository
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
                    Credits = 3,
                    CvText = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                
                dbTransaction.Create(userRef, newUser);
                
                DocumentReference txnRef = _db.Collection(FirestoreConstants.Collections.Transactions).Document();
                var txn = new Pharos.Core.Models.Transaction
                {
                    Id = txnRef.Id,
                    UserId = userId,
                    CreditsChanged = 3,
                    Reason = FirestoreConstants.TransactionReasons.WelcomeBalance,
                    CreatedAt = DateTime.UtcNow
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

        public async Task LockCreditAsync(string userId)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            
            await _db.RunTransactionAsync(async (Google.Cloud.Firestore.Transaction dbTransaction) =>
            {
                DocumentSnapshot snapshot = await dbTransaction.GetSnapshotAsync(userRef);
                if (!snapshot.Exists)
                {
                    throw new KeyNotFoundException($"User {userId} not found.");
                }
                
                var user = snapshot.ConvertTo<User>();
                
                // Concurrency lock check (expires in 5 minutes)
                if (user.CreditLockedAt != null && DateTime.UtcNow - user.CreditLockedAt.Value < TimeSpan.FromMinutes(5))
                {
                    throw new InvalidOperationException("An AI generation is already in progress.");
                }
                
                if (user.CreditLockedAt != null)
                {
                    // Lock is expired. Reuse the existing deduction, just refresh timestamp.
                    dbTransaction.Update(userRef, "creditLockedAt", DateTime.UtcNow);
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
        }

        public async Task ConfirmCreditDeductionAsync(string userId)
        {
            DocumentReference userRef = _db.Collection(FirestoreConstants.Collections.Users).Document(userId);
            await userRef.UpdateAsync("creditLockedAt", null);
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
                        CreatedAt = DateTime.UtcNow
                    };
                    dbTransaction.Create(txnRef, txn);
                }
            });
        }

        public async Task AddCreditsFromPaymentAsync(string userId, int amount, string paymentId)
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
                    Reason = $"payment_purchase:{paymentId}",
                    CreatedAt = DateTime.UtcNow
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
    }
}
