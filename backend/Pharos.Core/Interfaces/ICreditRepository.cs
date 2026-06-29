using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface ICreditRepository
    {
        Task<long> GetUserCreditsAsync(string userId);
        Task<string> LockCreditAsync(string userId);
        Task ConfirmCreditDeductionAsync(string userId, string transactionId, int promptTokens, int completionTokens);
        Task ReleaseCreditLockAsync(string userId);
        Task AddCreditsFromPaymentAsync(string userId, int amount, string paymentId, decimal cost, string currency);
    }
}
