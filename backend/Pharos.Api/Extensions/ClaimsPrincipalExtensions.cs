using System.Security.Claims;

namespace System.Security.Claims
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? user?.FindFirst("user_id")?.Value 
                ?? string.Empty;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user?.FindFirst(ClaimTypes.Email)?.Value 
                ?? user?.FindFirst("email")?.Value 
                ?? string.Empty;
        }
    }
}
