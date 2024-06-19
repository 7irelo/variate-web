using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Variate.Models;

namespace Variate.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            public DbSet<Product> Products => Set<Product>();
            public DbSet<Category> Categories => Set<Category>();
            public DbSet<Order> Orders => Set<Orders>();
            public DbSet<OrderItem> OrderItems => Set<OrderItem>();
            public DbSet<Review> Reviews => Set<Review>();
            public DbSet<Payment> Payments => Set<Payment>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Category>().HasData(
                    new {Id = 1, Name = "Electronics"},
                    new {Id = 2, Name = "Home and kitchen"},
                    new {Id = 3, Name = "Fashion and beauty"},
                    new {Id = 4, Name = "Toys and games"},
                    new {Id = 5, Name = "Books and DVDs"},
                    new {Id = 6, Name = "Baby Products"},
                    new {Id = 7, Name = "Outdoor and sports equipment"},
                    new {Id = 8, Name = "Health and wellness products"},
                    new {Id = 9, Name = "Arts and craft supplies"},
                    new {Id = 10, Name = "Musical Instruments"}
                );
            }
        }
    }
}
