using PresentationApi.ModelsBD;

namespace PresentationCreator.interfaces;

public interface IGoogleService
{
    public Task<User> AuthenticateWithGoogleAsync(string googleId, string email, string name, string avatarUrl);
}