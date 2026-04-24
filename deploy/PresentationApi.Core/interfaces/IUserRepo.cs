using PresentationApi.ModelsBD;

namespace PresentationCreator.interfaces;

public interface IUserRepo
{
    Task<User> AddUserIfNotExist(User user);
    Task<User> GetUserById(int id);
    Task<User> UpdatePayment(int id, int yookassaid);
    Task<User> UpdateSubscription(int id, int subscriptionId, DateTime date);
}