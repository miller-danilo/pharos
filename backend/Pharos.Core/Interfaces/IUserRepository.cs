using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetOrCreateUserAsync(string userId, string email);
        Task<System.Collections.Generic.List<Transaction>> GetUserTransactionsAsync(string userId);
        Task LogTransactionAsync(Transaction transaction);
        Task UpdateUserCvAsync(string userId, string cvText);
        Task<string> GetUserCvAsync(string userId);
    }
}
