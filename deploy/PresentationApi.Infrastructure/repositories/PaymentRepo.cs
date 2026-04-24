using Microsoft.EntityFrameworkCore;
using PresentationApi.Data;
using PresentationApi.ModelsBD;
using PresentationCreator.interfaces;

namespace PresentationApi.Infrastructure.repositories;

public class PaymentRepo : IPaymentRepo
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<Payment> _dbSet;

    public PaymentRepo(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<Payment>();
    }
    
    public async Task<Payment> Create(Payment payment)
    {
        try
        {
            var res = await _dbSet.AddAsync(payment);
            _context.SaveChanges();
            return res.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task Update(Payment payment)
    {
        try
        {
             _dbSet.Update(payment);
             await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Payment>> GetAllPayments(string idUser)
    {
        try
        {
            List<Payment> result = await _dbSet.Where(payment => payment.UserId.Equals(idUser)).ToListAsync();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Payment?> GetPayment(string paymentId)
    {
        try
        {
            Payment result = await _dbSet.Where(p => p.Id.Equals(paymentId)).FirstOrDefaultAsync();
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}