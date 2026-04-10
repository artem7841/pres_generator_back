using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;
using PresentationCreator.Models;

namespace PresentationCreator;

public class JwtGenerator : IJwtGenerator
{
    private readonly AuthOptions _authOptions;

    public JwtGenerator(IOptions<AuthOptions> authOptions)
    {
        _authOptions = authOptions.Value;
    }
    public JwtSecurityToken GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("provider", "google")
        };
            
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.SecretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _authOptions.Issuer,
            audience: _authOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromDays(_authOptions.TokenLifetime)),
            signingCredentials: signingCredentials);
        return jwt;
    }
}