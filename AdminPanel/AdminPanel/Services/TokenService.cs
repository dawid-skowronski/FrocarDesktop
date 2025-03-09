using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;


namespace AdminPanel.Services
{
    public static class TokenService
    {
        public static string Token { get; private set; }

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
            if (string.IsNullOrEmpty(Token)) return 0;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(Token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameidentifier")?.Value;

            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }

}
