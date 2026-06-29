using Pharos.Core.Models;
using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IConfigRepository
    {
        Task<CostMultipliers> GetCostMultipliersAsync();
        Task SaveCostMultipliersAsync(CostMultipliers multipliers);
    }
}
