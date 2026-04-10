using PresentationApi.ModelsBD;

namespace PresentationCreator.interfaces;

public interface IUserRepo
{
    Task<User> AddUserIfNotExist(User user);
}