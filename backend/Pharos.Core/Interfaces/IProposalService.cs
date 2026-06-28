using System.Threading.Tasks;

namespace Pharos.Core.Interfaces
{
    public interface IProposalService
    {
        Task<string> GenerateProposalWithCreditControlAsync(string userId, string cvText, string jobText);
    }
}
