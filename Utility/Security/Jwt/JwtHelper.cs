using Core.ResultType;
using Core.Security.Encyption;
using DataAccessLayer.WMDbContext;
using DataObject.Helpers;
using DataObject.WM;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Utility.Security.Jwt
{
    public class JwtHelper : ITokenHelper
    {
        private readonly AppSettings _appSettings;
        public IConfiguration _configuration { get; }
        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>();
        }
        public Result<AccessToken> CreateToken(int id)
        {
            Result<AccessToken> tokenobject;
            AccessToken accessToken = new AccessToken();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            try
            {

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.Name, id.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                accessToken.Token = tokenHandler.WriteToken(token);
                tokenobject = new Result<AccessToken>(true, ResultTypeEnum.Success, accessToken, "Token oluşturma başarılı.");

            }
            catch (Exception ex)
            {
                tokenobject = new Result<AccessToken>(false, ResultTypeEnum.Error, accessToken, "Token oluşturma başarısız.");
            }
            return tokenobject;
        }
    }
}
