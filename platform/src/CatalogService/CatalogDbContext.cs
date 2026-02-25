using Microsoft.EntityFrameworkCore;
using Variate.CatalogService.Models;

namespace Variate.CatalogService;

public class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductEntity>()
            .HasIndex(x => x.Name);

        modelBuilder.Entity<CategoryEntity>()
            .HasIndex(x => x.Name)
            .IsUnique();
    }
}
