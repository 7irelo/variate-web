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
            new {Id = 4, Name = "Pop Punk"},
            new {Id = 5, Name = "Disco"},
            new {Id = 6, Name = "Electronic Dance Music"},
            new {Id = 7, Name = "Dark Synth Pop"}
        );
    }
}
