using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace AdminPanel.Services
{
    public static class TokenService
    {
        private const string NameIdentifierClaim = "nameidentifier";
        private const string LegacyNameIdentifierClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public static string? Token { get; private set; }

        public static void SetToken(string token)
        {
            Token = token;
        }

        public static void ClearToken()
        {
            Token = null;
        }

        public static int GetUserId()
        {
            if (string.IsNullOrEmpty(Token))
            {
                return 0; 
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(Token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == NameIdentifierClaim) ??
                                  jwtToken.Claims.FirstOrDefault(c => c.Type == LegacyNameIdentifierClaim);

                return int.TryParse(userIdClaim?.Value, out int userId) ? userId : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}