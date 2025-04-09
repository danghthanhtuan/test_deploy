using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApi.DTO;
using WebApi.Models;
namespace WebApi.Service.Admin
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<LoginResponseModelclient?> CreateTokenUser(string Nameclient)
        {
            try
            {
                var issuer = _configuration["JwtConfig:Issuer"];
                var audience = _configuration["JwtConfig:Audience"];
                var key = _configuration["JwtConfig:Key"];
                var tokenValidityMins = _configuration.GetValue<int>("JwtConfig:TokenValidityMins");
                var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Name, Nameclient!),
                        new Claim(ClaimTypes.Role, "User")
                    }),
                    Expires = tokenExpiryTimeStamp,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)), SecurityAlgorithms.HmacSha256Signature),
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(securityToken);

                return new LoginResponseModelclient
                {
                    AccessToken = accessToken,
                    SDT = Nameclient,
                    ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds,
                };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và trả về phản hồi thích hợp
                throw new InvalidOperationException("Error creating JWT token", ex);
            }
        }

        public async Task<LoginResponseModel?> CreateTokenAdmin(string NameNV, string Department)
        {
            try
            {
                var issuer = _configuration["JwtConfig:Issuer"];
                var audience = _configuration["JwtConfig:Audience"];
                var key = _configuration["JwtConfig:Key"];
                var tokenValidityMins = _configuration.GetValue<int>("JwtConfig:TokenValidityMins");
                var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Name, NameNV!),
                        new Claim("Department", Department), // Sử dụng chuỗi tùy chỉnh thay vì JwtRegisteredClaimNames
                        new Claim(ClaimTypes.Role, "Admin")
                    }),
                    Expires = tokenExpiryTimeStamp,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)), SecurityAlgorithms.HmacSha256Signature),
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var securityToken = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(securityToken);

                return new LoginResponseModel
                {
                    AccessToken = accessToken,
                    SDT = NameNV,
                    Department = Department,
                    ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds,
                };
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và trả về phản hồi thích hợp
                throw new InvalidOperationException("Error creating JWT token", ex);
            }
        }
        //public async Task<LoginResponseModel1?> CreateTokenAdmin(string NameNV)
        //{
        //    try
        //    {
        //        var issuer = _configuration["JwtConfig:Issuer"];
        //        var audience = _configuration["JwtConfig:Audience"];
        //        var key = _configuration["JwtConfig:Key"];
        //        var tokenValidityMins = _configuration.GetValue<int>("JwtConfig:TokenValidityMins");
        //        var refreshTokenValidityDays = _configuration.GetValue<int>("JwtConfig:RefreshTokenValidityDays");

        //        var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMins);
        //        var refreshTokenExpiry = DateTime.UtcNow.AddDays(refreshTokenValidityDays);

        //        var tokenDescriptor = new SecurityTokenDescriptor
        //        {
        //            Subject = new ClaimsIdentity(new[]
        //            {
        //        new Claim(JwtRegisteredClaimNames.Name, NameNV!),
        //        new Claim(ClaimTypes.Role, "Admin")
        //    }),
        //            Expires = tokenExpiryTimeStamp,
        //            Issuer = issuer,
        //            Audience = audience,
        //            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)), SecurityAlgorithms.HmacSha256Signature),
        //        };

        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        //        var accessToken = tokenHandler.WriteToken(securityToken);

        //        // Sinh Refresh Token
        //        var refreshToken = Guid.NewGuid().ToString();

        //        return new LoginResponseModel1
        //        {
        //            AccessToken = accessToken,
        //            RefreshToken = refreshToken,
        //            SDT = NameNV,
        //            ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.UtcNow).TotalSeconds,
        //            RefreshTokenExpiry = refreshTokenExpiry
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new InvalidOperationException("Error creating JWT token", ex);
        //    }
        //}

    }
}
