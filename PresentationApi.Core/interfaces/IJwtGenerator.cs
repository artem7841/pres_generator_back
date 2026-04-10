using System.IdentityModel.Tokens.Jwt;
using PresentationApi.ModelsBD;

namespace PresentationCreator.interfaces;

public interface IJwtGenerator
{
    public  JwtSecurityToken GenerateJwtToken(User user);
}