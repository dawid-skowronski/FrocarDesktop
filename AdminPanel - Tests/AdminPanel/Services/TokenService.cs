using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace AdminPanel.Services
{
    public static partial class TokenService
    {
        public static string Token { get; private set; }
        public static Func<int>? GetUserIdDelegate { get; set; }

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
            if (GetUserIdDelegate != null)
            {
                return GetUserIdDelegate();
            }

            if (string.IsNullOrEmpty(Token)) return 0;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(Token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameidentifier")?.Value
                ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}