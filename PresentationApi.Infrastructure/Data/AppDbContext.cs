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
            var connectionString = Environment.GetEnvironmentVariable("DOCKER_RUNNING") == "true"
                ? "Data Source=/app/data/app.db"
                : "Data Source=app.db";
            optionsBuilder.UseSqlite(connectionString);
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LoginCode>()
            .HasOne(l => l.User)
            .WithMany(u => u.LoginCodes)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull); 
            
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); 
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique(); 

        modelBuilder.Entity<PresProject>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId);
        
        modelBuilder.Entity<LoginCode>()
            .HasIndex(l => l.Email);
    }
}