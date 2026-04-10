using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;

namespace PresentationCreator;

public class GoogleService : IGoogleService
{
    private IUserRepo _userRepo;

    public GoogleService(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<User> AuthenticateWithGoogleAsync(string googleId, string email, string name, string avatarUrl)
    {
        var user = new User()
        {
            Email = email,
            Name = name,
            AvatarUrl = avatarUrl,
            GoogleId = googleId,
            Provider = "google",
            CreatedAt = DateTime.UtcNow,
        };

        User answer = await _userRepo.AddUserIfNotExist(user);
        return answer;
    }
}