using Pharos.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pharos.Core.Services
{
    // C# 12 Primary Constructor
    public class ProposalService(IUserRepository userRepo, IAiService aiService) : IProposalService
    {
        public async Task<string> GenerateProposalWithCreditControlAsync(string userId, string cvText, string jobText)
        {
            if (string.IsNullOrEmpty(cvText))
            {
                cvText = await userRepo.GetUserCvAsync(userId);
            }

            if (string.IsNullOrEmpty(cvText))
            {
                throw new ArgumentException("User CV must be uploaded first.");
            }

            // Step 1: Lock credit (Lease / Lock)
            await userRepo.LockCreditAsync(userId);

            try
            {
                // Step 2: Generate proposal from AI
                string proposal = await aiService.GenerateProposalAsync(cvText, jobText);

                // Step 3: Confirm credit deduction (Release lock - finalized!)
                await userRepo.ConfirmCreditDeductionAsync(userId);

                return proposal;
            }
            catch (Exception)
            {
                try
                {
                    // Step 4: Release lock (Refund credit)
                    await userRepo.ReleaseCreditLockAsync(userId);
                }
                catch (Exception refundEx)
                {
                    Console.WriteLine($"Critical: Failed to release credit lock for user {userId}: {refundEx.Message}");
                }
                throw;
            }
        }
    }
}
