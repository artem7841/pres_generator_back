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

    public async Task<User> AddUserIfNotExist(string email)
    {
        var userBd = await _dbSet.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (userBd != null)
        {
            return userBd;
        } 
        else
        {
            User user = new User();
            user.Email = email;
            user.CreatedAt = DateTime.Now;
            _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        } 
    }
}