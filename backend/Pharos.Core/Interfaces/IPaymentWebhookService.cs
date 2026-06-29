using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IPaymentWebhookService
    {
        Task ProcessWebhookAsync(string requestBody, string? signature);
    }
}
