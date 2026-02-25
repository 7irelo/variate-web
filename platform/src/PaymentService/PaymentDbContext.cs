using Microsoft.EntityFrameworkCore;
using Variate.PaymentService.Models;

namespace Variate.PaymentService;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentEntity>()
            .HasIndex(p => p.OrderId);

        modelBuilder.Entity<PaymentEntity>()
            .HasIndex(p => p.Status);
    }
}
