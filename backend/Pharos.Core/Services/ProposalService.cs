using Pharos.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Pharos.Core.Services
{
    // C# 12 Primary Constructor
    public class ProposalService(IUserRepository userRepo, ICreditRepository creditRepo, IAiService aiService, ILogger<ProposalService> logger) : IProposalService
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
            string transactionId = await creditRepo.LockCreditAsync(userId);

            try
            {
                // Step 2: Generate proposal from AI
                var proposalResult = await aiService.GenerateProposalAsync(cvText, jobText);

                // Step 3: Confirm credit deduction (Release lock - finalized!)
                await creditRepo.ConfirmCreditDeductionAsync(userId, transactionId, proposalResult.PromptTokens, proposalResult.CompletionTokens);

                return proposalResult.ProposalText;
            }
            catch (Exception)
            {
                try
                {
                    // Step 4: Release lock (Refund credit)
                    await creditRepo.ReleaseCreditLockAsync(userId);
                }
                catch (Exception refundEx)
                {
                    logger.LogCritical(refundEx, "Critical: Failed to release credit lock for user {UserId}", userId);
                }
                throw;
            }
        }
    }
}
