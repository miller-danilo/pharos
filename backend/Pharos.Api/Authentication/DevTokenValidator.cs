using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Pharos.Api.Authentication
{
    public class DevTokenValidator : ISecurityTokenValidator
    {
        public bool CanReadToken(string securityToken) => true;

        public bool CanValidateToken => true;

        public ClaimsPrincipal ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(securityToken);
            validatedToken = jwtToken;

            var identity = new ClaimsIdentity(jwtToken.Claims, "Bearer");
            return new ClaimsPrincipal(identity);
        }

        public bool CanWriteToken => false;
        public int MaximumTokenSizeInBytes { get; set; } = int.MaxValue;
    }
}
