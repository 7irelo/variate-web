using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using variate.Models;

namespace variate.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seeding categories
            modelBuilder.Entity<Category>().HasData(
                new { Id = 1, Name = "Electronics", Description = "Various electronic gadgets" },
                new { Id = 2, Name = "Home and Kitchen", Description = "Home appliances and kitchenware" },
                new { Id = 3, Name = "Fashion and Beauty", Description = "Fashion and beauty products" },
                new { Id = 4, Name = "Musical Instruments", Description = "Instruments for making music" },
                new { Id = 5, Name = "Art and Crafts", Description = "Art and craft supplies" },
                new { Id = 6, Name = "Baby and Toddler", Description = "Products for babies and toddlers" },
                new { Id = 7, Name = "Bed and Bath", Description = "Bedding and bath essentials" },
                new { Id = 8, Name = "Decor and Furniture", Description = "Home decor and furniture" },
                new { Id = 9, Name = "Health and Beauty", Description = "Health and beauty products" },
                new { Id = 10, Name = "Home and Garden", Description = "Garden tools and supplies" },
                new { Id = 11, Name = "Jewellery and Watches", Description = "Jewellery and watches" },
                new { Id = 12, Name = "Luggage and Travel", Description = "Luggage and travel accessories" },
                new { Id = 13, Name = "Office and Stationery", Description = "Office supplies and stationery" },
                new { Id = 14, Name = "Pet Products", Description = "Products for pets" },
                new { Id = 15, Name = "Sports and Outdoor", Description = "Sports and outdoor equipment" },
                new { Id = 16, Name = "Tools and DIY", Description = "Tools for DIY projects" },
                new { Id = 17, Name = "Toys and Games", Description = "Toys and games for children" }
            );

            // Seeding products
            modelBuilder.Entity<Product>().HasData(
                new { Id = 1, CategoryId = 1, Name = "Smartphone", Description = "A modern smartphone with all the latest features.", Price = 5999m, Release = new DateOnly(2024, 1, 1), ImageUrl = "smartphone.jpg" },
                new { Id = 2, CategoryId = 1, Name = "Laptop", Description = "A high-performance laptop for gaming and work.", Price = 14999m, Release = new DateOnly(2024, 2, 1), ImageUrl = "laptop.jpg" },
                new { Id = 3, CategoryId = 2, Name = "Blender", Description = "A powerful blender for making smoothies and more.", Price = 1299m, Release = new DateOnly(2024, 3, 1), ImageUrl = "blender.jpg" },
                new { Id = 4, CategoryId = 2, Name = "Microwave", Description = "A compact microwave with multiple cooking functions.", Price = 1999m, Release = new DateOnly(2024, 4, 1), ImageUrl = "microwave.jpg" },
                new { Id = 5, CategoryId = 3, Name = "Lipstick", Description = "A long-lasting lipstick available in various shades.", Price = 199m, Release = new DateOnly(2024, 5, 1), ImageUrl = "lipstick.jpg" },
                new { Id = 6, CategoryId = 3, Name = "Perfume", Description = "A luxurious fragrance for special occasions.", Price = 799m, Release = new DateOnly(2024, 6, 1), ImageUrl = "perfume.jpg" }
            );

            modelBuilder.Entity<Order>()
                .HasOne(o => o.IdentityUser)
                .WithMany()  // IdentityUser can have multiple Orders
                .HasForeignKey(o => o.IdentityUserId)
                .OnDelete(DeleteBehavior.Cascade);  // Set appropriate delete behavior
        }
    }
}
