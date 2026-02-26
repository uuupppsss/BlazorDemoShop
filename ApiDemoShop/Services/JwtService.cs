using ApiDemoShop.Data;
using ApiDemoShop.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiDemoShop.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var claims = new List<Claim>()
                {
                    new Claim(ClaimValueTypes.Integer32, user.Id.ToString()),
                    new Claim(ClaimTypes.Name,user.Username),
                    //new Claim (ClaimTypes.Role, found_user.Role.Title),
                };

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                //кладём полезную нагрузку
                claims: claims,
                //устанавливаем время жизни токена 30
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

            string token = new JwtSecurityTokenHandler().WriteToken(jwt);
            return token;

        }

        //public int? ValidateToken(string token)
        //{
        //    if (string.IsNullOrEmpty(token))
        //        return null;

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

        //    try
        //    {
        //        tokenHandler.ValidateToken(token, new TokenValidationParameters
        //        {
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(key),
        //            ValidateIssuer = true,
        //            ValidIssuer = _configuration["Jwt:Issuer"],
        //            ValidateAudience = true,
        //            ValidAudience = _configuration["Jwt:Audience"],
        //            ValidateLifetime = true,
        //            ClockSkew = TimeSpan.Zero
        //        }, out SecurityToken validatedToken);

        //        var jwtToken = (JwtSecurityToken)validatedToken;
        //        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

        //        return userId;
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}
    }

}
