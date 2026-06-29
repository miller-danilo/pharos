using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetOrCreateUserAsync(string userId, string email);
        Task<long> GetUserCreditsAsync(string userId);
        Task<string> LockCreditAsync(string userId);
        Task ConfirmCreditDeductionAsync(string userId, string transactionId, int promptTokens, int completionTokens);
        Task ReleaseCreditLockAsync(string userId);
        Task AddCreditsFromPaymentAsync(string userId, int amount, string paymentId, decimal cost, string currency);
        Task<System.Collections.Generic.List<Transaction>> GetUserTransactionsAsync(string userId);
        Task LogTransactionAsync(Transaction transaction);
        Task<CostMultipliers> GetCostMultipliersAsync();
        Task SaveCostMultipliersAsync(CostMultipliers multipliers);
        Task UpdateUserCvAsync(string userId, string cvText);
        Task<string> GetUserCvAsync(string userId);
    }
}
