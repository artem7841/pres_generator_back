using Microsoft.EntityFrameworkCore;
using PresentationApi.Data;
using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;

namespace PresentationApi.Infrastructure.repositories;

public class CodeRepo : ICodeRepo
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<LoginCode> _dbSet;

    public CodeRepo(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<LoginCode>();
    }


    public async Task<bool> AddCode(string code, string email)
    {
        try
        {
            await _dbSet.AddAsync(new LoginCode { Code = code, 
                Email = email,  
                IsUsed = false, 
                CreatedAt = DateTime.Now, 
                ExpiresAt = DateTime.Now + TimeSpan.FromMinutes(3)});
            await _context.SaveChangesAsync();
        } 
        catch (Exception e)
        {
            return false;
        }
        return true;
    }

    public Task<LoginCode> GetLastCode(string email)
    {
        Console.WriteLine(email);
        var listCodes = _dbSet
            .Where(x => x.Email == email)
            .Where(x =>  x.IsUsed == false)
            .OrderByDescending(exp => exp.CreatedAt)
            .FirstOrDefaultAsync();
        
        return listCodes;
    }
}