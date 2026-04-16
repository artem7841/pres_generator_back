
using PresentationApi.ModelsBD;
using PresentationCreator.Models;

namespace PresentationCreator.interfaces;

public interface IAuthService
{
    public Task<EmailAnswer> SendCodeOnEmail(string email);
    public Task<JwtTokens> ApproveCode(string email, string code);
    public Task<RefreshTokenAnswer> RefreshToken(string refreshToken);
    public Task Logout(string token);
    public Task<User> GetUser(string id);
}