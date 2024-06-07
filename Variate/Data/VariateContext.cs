using Microsoft.EntityFrameworkCore;
using Variate.Entities;

namespace Variate.Data;

public class VariateContext(DbContextOptions<VariateContext> options): DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Genre>().HasData(
            new {Id = 1, Name = "Neo Soul"},
            new {Id = 2, Name = "West Coast Rap"},
            new {Id = 3, Name = "Rhythm and Blues"},
            new {Id = 4, Name = "Dark Synth Pop"}
        );
    }
}
