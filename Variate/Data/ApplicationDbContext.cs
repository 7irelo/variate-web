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
                    new {Id = 2, Name = "Home and Kitchen"},
                    new {Id = 3, Name = "Fashion and Beauty"},
                    new {Id = 4, Name = "Musical Instruments"},
                    new {Id = 5, Name = "Art and Crafts"},
                    new {Id = 6, Name = "Baby and Toddler"},
                    new {Id = 7, Name = "Bed and Bath"},
                    new {Id = 8, Name = "Decor and Furniture"},
                    new {Id = 9, Name = "Health and Beauty"},
                    new {Id = 10, Name = "Home and Garden"},
                    new {Id = 11, Name = "Jewellery and Watches"},
                    new {Id = 12, Name = "Luggage and Travel"},
                    new {Id = 13, Name = "Office and Stationery"},
                    new {Id = 14, Name = "Pet Products"},
                    new {Id = 15, Name = "Sports and Outdoor"},
                    new {Id = 16, Name = "Tools and DIY"},
                    new {Id = 17, Name = "Toys and Games"}
                );
            }
        }
    }
}
