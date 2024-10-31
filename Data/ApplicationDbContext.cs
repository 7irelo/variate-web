using System;
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
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure one-to-many relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            // Initialize Random object
            var random = new Random();

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
                // Electronics
                new { Id = 1, CategoryId = 1, Brand = "Samsung", Name = "A15 Blue", Description = "A sleek smartphone with a powerful processor.", Price = 5999m, DiscountedPrice = 4999m, Release = new DateOnly(2024, 1, 1), ImageUrl = "samsung_a15_blue.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 2, CategoryId = 1, Brand = "Dell", Name = "XPS 15", Description = "A high-performance laptop with a stunning display.", Price = 21999m, DiscountedPrice = 18999m, Release = new DateOnly(2024, 2, 1), ImageUrl = "dell_xps_15.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 3, CategoryId = 1, Brand = "Sony", Name = "WH-1000XM4", Description = "Noise-cancelling wireless headphones.", Price = 3999m, DiscountedPrice = 3499m, Release = new DateOnly(2024, 3, 1), ImageUrl = "sony_wh_1000xm4.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 4, CategoryId = 1, Brand = "Apple", Name = "iPad Air", Description = "A versatile tablet with a powerful A14 chip.", Price = 9999m, DiscountedPrice = 8999m, Release = new DateOnly(2024, 4, 1), ImageUrl = "ipad_air.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 5, CategoryId = 1, Brand = "Canon", Name = "EOS R6", Description = "A full-frame mirrorless camera for professional photography.", Price = 36999m, DiscountedPrice = 31999m, Release = new DateOnly(2024, 5, 1), ImageUrl = "canon_eos_r6.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Home and Kitchen
                new { Id = 6, CategoryId = 2, Brand = "Philips", Name = "Air Fryer", Description = "A healthy way to fry food with little to no oil.", Price = 2799m, DiscountedPrice = 2299m, Release = new DateOnly(2024, 1, 1), ImageUrl = "philips_air_fryer.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 7, CategoryId = 2, Brand = "Dyson", Name = "V11 Vacuum Cleaner", Description = "A powerful cordless vacuum cleaner.", Price = 7999m, DiscountedPrice = 6999m, Release = new DateOnly(2024, 2, 1), ImageUrl = "dyson_v11.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 8, CategoryId = 2, Brand = "KitchenAid", Name = "Stand Mixer", Description = "A versatile mixer for all your baking needs.", Price = 6999m, DiscountedPrice = 5499m, Release = new DateOnly(2024, 3, 1), ImageUrl = "kitchenaid_mixer.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 9, CategoryId = 2, Brand = "Nespresso", Name = "Coffee Machine", Description = "A convenient way to make espresso at home.", Price = 3499m, DiscountedPrice = 2999m, Release = new DateOnly(2024, 4, 1), ImageUrl = "nespresso_machine.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 10, CategoryId = 2, Brand = "Instant", Name = "Pot Duo", Description = "A multi-use pressure cooker for quick meals.", Price = 1599m, DiscountedPrice = 1199m, Release = new DateOnly(2024, 5, 1), ImageUrl = "instant_pot_duo.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Fashion and Beauty
                new { Id = 11, CategoryId = 3, Brand = "Chanel", Name = "No. 5 Perfume", Description = "A timeless fragrance for special occasions.", Price = 1999m, DiscountedPrice = 1699m, Release = new DateOnly(2024, 1, 1), ImageUrl = "chanel_no_5.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 12, CategoryId = 3, Brand = "Maybelline", Name = "Mascara", Description = "A volumizing mascara for bold lashes.", Price = 199m, DiscountedPrice = 129m, Release = new DateOnly(2024, 2, 1), ImageUrl = "maybelline_mascara.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 13, CategoryId = 3,  Brand = "Gucci", Name = "Handbag", Description = "A luxury handbag made from fine leather.", Price = 15999m, DiscountedPrice = 13999m, Release = new DateOnly(2024, 3, 1), ImageUrl = "gucci_handbag.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 14, CategoryId = 3, Brand = "Ray-Ban",  Name = "Aviator Sunglasses", Description = "Classic sunglasses with polarized lenses.", Price = 2999m, DiscountedPrice = 2499m, Release = new DateOnly(2024, 4, 1), ImageUrl = "rayban_aviator.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 15, CategoryId = 3, Brand = "MAC",  Name = "Lipstick", Description = "A long-lasting lipstick in a range of colors.", Price = 299m, DiscountedPrice = 249m, Release = new DateOnly(2024, 5, 1), ImageUrl = "mac_lipstick.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Musical Instruments
                new { Id = 16, CategoryId = 4, Brand = "Yamaha",  Name = "Acoustic Guitar", Description = "A high-quality acoustic guitar for all levels.", Price = 4999m, DiscountedPrice = 4499m, Release = new DateOnly(2024, 1, 1), ImageUrl = "yamaha_acoustic_guitar.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 17, CategoryId = 4, Brand = "Roland",  Name = "Digital Piano", Description = "A digital piano with realistic touch and sound.", Price = 19999m, DiscountedPrice = 17999m, Release = new DateOnly(2024, 2, 1), ImageUrl = "roland_digital_piano.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 18, CategoryId = 4, Brand = "Fender",  Name = "Stratocaster", Description = "A legendary electric guitar for professionals.", Price = 17999m, DiscountedPrice = 12999m, Release = new DateOnly(2024, 3, 1), ImageUrl = "fender_stratocaster.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 19, CategoryId = 4, Brand = "Zildjian",  Name = "Cymbals", Description = "Professional cymbals for drummers.", Price = 5999m, DiscountedPrice = 4999m, Release = new DateOnly(2024, 4, 1), ImageUrl = "zildjian_cymbals.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 20, CategoryId = 4, Brand = "Shure",  Name = "SM58 Microphone", Description = "A reliable microphone for live performances.", Price = 1999m, DiscountedPrice = 1499m, Release = new DateOnly(2024, 5, 1), ImageUrl = "shure_sm58.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Art and Crafts
                new { Id = 21, CategoryId = 5, Brand = "Winsor & Newton",  Name = "Watercolors", Description = "High-quality watercolor paints for artists.", Price = 999m, DiscountedPrice = 799m, Release = new DateOnly(2024, 1, 1), ImageUrl = "winsor_newton_watercolors.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 22, CategoryId = 5, Brand = "Faber-Castell", Name = "Pencils", Description = "A set of professional-grade colored pencils.", Price = 599m, DiscountedPrice = 499m, Release = new DateOnly(2024, 2, 1), ImageUrl = "faber_castell_pencils.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 23, CategoryId = 5, Brand = "Cricut", Name = "Maker", Description = "A versatile cutting machine for crafting.", Price = 4999m, DiscountedPrice = 3999m, Release = new DateOnly(2024, 3, 1), ImageUrl = "cricut_maker.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 24, CategoryId = 5, Brand = "Sennelier", Name = "Oil Pastels", Description = "Richly pigmented oil pastels for artists.", Price = 1999m, DiscountedPrice = 1599m, Release = new DateOnly(2024, 4, 1), ImageUrl = "sennelier_oil_pastels.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new { Id = 25, CategoryId = 5, Brand = "Canson",  Name = "Drawing Paper", Description = "High-quality drawing paper for all mediums.", Price = 499m, DiscountedPrice = 429m, Release = new DateOnly(2024, 5, 1), ImageUrl = "canson_drawing_paper.jpg", Stock = 0, IsFeatured = false, OnSale = random.Next(0, 2) == 1, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }            );
        }
    }
}
