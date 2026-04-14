using Microsoft.EntityFrameworkCore;
using PresentationApi.ModelsBD;

namespace PresentationApi.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<LoginCode> LoginCodes { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PresProject> Files { get; set; }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public AppDbContext() : base()
    {
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Настройка внешних ключей и индексов
        modelBuilder.Entity<LoginCode>()
            .HasOne(l => l.User)
            .WithMany(u => u.LoginCodes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull); // Если пользователя удалят, коды останутся, но UserId станет NULL
            
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Если пользователя удалят, удалятся и его платежи
            
        // Индекс для быстрого поиска по email (ускоряет авторизацию)
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique(); // email не должен повторяться

        modelBuilder.Entity<PresProject>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);
            
        // Индекс для быстрого поиска кодов по email
        modelBuilder.Entity<LoginCode>()
            .HasIndex(l => l.Email);
    }
}