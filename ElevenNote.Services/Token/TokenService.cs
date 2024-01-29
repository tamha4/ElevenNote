

using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ElevenNote.Data.Entities;
using ElevenNote.Models.Token;
using ElevenNote.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace ElevenNote.Services.Token
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<UserEntity> _userManager;
        public TokenService(IConfiguration configuration, UserManager<UserEntity> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;            
        }
        public async Task<TokenResponse?> GetTokenAsync(TokenRequest model){
            UserEntity? entity = await GetValidUserAsync(model);

            if(entity is null){
                return null;
            }
            return await GenerateTokenAsync(entity);
        }

        private async Task<UserEntity?> GetValidUserAsync(TokenRequest model){
            var userEntity = await _userManager.FindByNameAsync(model.UserName);

            if(userEntity is null){
                return null;
            }
            var isValidPassword = await _userManager.CheckPasswordAsync(userEntity, model.Password);
            if(isValidPassword == false)
            return null;

            return userEntity;
        }
private async Task<TokenResponse> GenerateTokenAsync(UserEntity entity)
{
    List<Claim> claims = await GetUserClaimsAsync(entity);
    SecurityTokenDescriptor tokenDescriptor = GetTokenDescriptor(claims);

    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);

    TokenResponse response = new TokenResponse()
    {
        Token = tokenHandler.WriteToken(token),
        IssuedAt = token.ValidFrom,
        Expires = token.ValidTo
    };
    return response;
}


        private async Task<List<Claim>> GetUserClaimsAsync(UserEntity entity){
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString()),
                new Claim(ClaimTypes.Name, entity.UserName!),
                new Claim(ClaimTypes.Email, entity.Email!)
            };

            // only add this section if you previuosly added the Role Configuration in program.cs file
            // the configuration by .addRoles<IdentityRole<int>>() chained to the .AddDefaultIdentity method
            var roles = await _userManager.GetRolesAsync(entity);
            foreach( var role in roles){
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
            
        }

        private SecurityTokenDescriptor GetTokenDescriptor(List<Claim> claims){
            var newKeyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(newKeyBytes);
            }
            string newKeyBase64 = Convert.ToBase64String(newKeyBytes);

            _configuration["Jwt:Key"] = newKeyBase64;        


            // var key = Encoding.UTF8.GetBytes(newKeyBytes);
            var secret = new SymmetricSecurityKey(newKeyBytes);
            var signingCredentials = new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);

            SecurityTokenDescriptor tokenDescriptor = new(){
                Issuer = _configuration["Jwt:Issuer"]!,
                Audience = _configuration["Jwt:Audience"]!,
                Subject = new ClaimsIdentity(claims),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = signingCredentials
            };
            return tokenDescriptor;
        }

    }
}