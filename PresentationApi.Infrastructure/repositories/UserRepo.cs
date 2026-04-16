using Microsoft.EntityFrameworkCore;
using PresentationApi.Data;
using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;

namespace PresentationApi.Infrastructure.repositories;

public class UserRepo : IUserRepo
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<User> _dbSet;

    public UserRepo(AppDbContext context)
    {
        _context = context;
        _dbSet = _dbSet = context.Set<User>();;
    }

    public async Task<User> AddUserIfNotExist(User user)
    {
        var userBd = await _dbSet.Where(u => u.Email == user.Email).FirstOrDefaultAsync();
        if (userBd != null)
        {
            userBd.Name = user.Name;
            userBd.AvatarUrl = user.AvatarUrl;
            userBd.GoogleId = user.GoogleId;

        
            await _context.SaveChangesAsync();
            return userBd;
        } 
        else
        {
            User newUser = new User();
            newUser.Id = Guid.NewGuid().GetHashCode();
            newUser.Email = user.Email;
            newUser.AvatarUrl = user.AvatarUrl;
            newUser.Name = user.Name;
            newUser.GoogleId = user.GoogleId;
            newUser.CreatedAt = DateTime.Now;
            await _dbSet.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        } 
    }

    public async Task<User?> GetUserById(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<User> UpdatePayment(int id, int paymentId)
    {
        var userBD = await _dbSet.Where(u => u.Id.Equals(id)).FirstOrDefaultAsync();
        userBD.YookassaPaymentMethodId = paymentId.ToString();
        await _context.SaveChangesAsync();
        return userBD;
    }

    public async Task<User> UpdateSubscription(int id, int paymentId, DateTime date)
    {
        var userBD = await _dbSet.Where(u => u.Id.Equals(id)).FirstOrDefaultAsync();
        userBD.HasActiveSubscription = true;
        userBD.SubscriptionExpiresAt =  date;
        await _context.SaveChangesAsync();
        return userBD;
    }
}