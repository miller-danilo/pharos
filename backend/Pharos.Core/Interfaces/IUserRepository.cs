using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetOrCreateUserAsync(string userId, string email);
        Task<long> GetUserCreditsAsync(string userId);
        Task LockCreditAsync(string userId);
        Task ConfirmCreditDeductionAsync(string userId);
        Task ReleaseCreditLockAsync(string userId);
        Task AddCreditsFromPaymentAsync(string userId, int amount, string paymentId);
        Task UpdateUserCvAsync(string userId, string cvText);
        Task<string> GetUserCvAsync(string userId);
    }
}
