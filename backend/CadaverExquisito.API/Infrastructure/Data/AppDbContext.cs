using CadaverExquisito.API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CadaverExquisito.API.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Cadaver> Cadavers => Set<Cadaver>();
    public DbSet<Fragment> Fragments => Set<Fragment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Cadaver>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasOne(c => c.CreatedByUser)
             .WithMany(u => u.CreatedCadavers)
             .HasForeignKey(c => c.CreatedByUserId);
        });

        modelBuilder.Entity<Fragment>(e =>
        {
            e.HasKey(f => f.Id);
            e.HasOne(f => f.Cadaver)
             .WithMany(c => c.Fragments)
             .HasForeignKey(f => f.CadaverId);
            e.HasOne(f => f.User)
             .WithMany(u => u.Fragments)
             .HasForeignKey(f => f.UserId);
            e.HasIndex(f => new { f.CadaverId, f.UserId }).IsUnique();
        });
    }
}
