using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IScanRepository
    {
        Task<Scan?> GetScanByHashAsync(string hash);
        Task SaveScanAsync(Scan scan);
    }
}
