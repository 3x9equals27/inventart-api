using Inventart.Authorization;
using Inventart.Config;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Inventart.Services.Singleton
{
    public class JwtService
    {
        private readonly JwtConfig _config;
        private readonly SymmetricSecurityKey _key;
        public JwtService(IOptions<JwtConfig> jwtConfig)
        {
            _config = jwtConfig.Value;
            _key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.Secret));
        }
        public string GenerateJwtToken(UserToken userToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userToken.GetClaims()),
                Expires = DateTime.UtcNow.AddDays(_config.Expires),
                SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public UserToken ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userToken = new UserToken(jwtToken.Claims.ToArray());

                return userToken;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
    }
}
