﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using variate.Data;

#nullable disable

namespace variate.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("variate.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Various electronic gadgets",
                            Name = "Electronics"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Home appliances and kitchenware",
                            Name = "Home and Kitchen"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Fashion and beauty products",
                            Name = "Fashion and Beauty"
                        },
                        new
                        {
                            Id = 4,
                            Description = "Instruments for making music",
                            Name = "Musical Instruments"
                        },
                        new
                        {
                            Id = 5,
                            Description = "Art and craft supplies",
                            Name = "Art and Crafts"
                        },
                        new
                        {
                            Id = 6,
                            Description = "Products for babies and toddlers",
                            Name = "Baby and Toddler"
                        },
                        new
                        {
                            Id = 7,
                            Description = "Bedding and bath essentials",
                            Name = "Bed and Bath"
                        },
                        new
                        {
                            Id = 8,
                            Description = "Home decor and furniture",
                            Name = "Decor and Furniture"
                        },
                        new
                        {
                            Id = 9,
                            Description = "Health and beauty products",
                            Name = "Health and Beauty"
                        },
                        new
                        {
                            Id = 10,
                            Description = "Garden tools and supplies",
                            Name = "Home and Garden"
                        },
                        new
                        {
                            Id = 11,
                            Description = "Jewellery and watches",
                            Name = "Jewellery and Watches"
                        },
                        new
                        {
                            Id = 12,
                            Description = "Luggage and travel accessories",
                            Name = "Luggage and Travel"
                        },
                        new
                        {
                            Id = 13,
                            Description = "Office supplies and stationery",
                            Name = "Office and Stationery"
                        },
                        new
                        {
                            Id = 14,
                            Description = "Products for pets",
                            Name = "Pet Products"
                        },
                        new
                        {
                            Id = 15,
                            Description = "Sports and outdoor equipment",
                            Name = "Sports and Outdoor"
                        },
                        new
                        {
                            Id = 16,
                            Description = "Tools for DIY projects",
                            Name = "Tools and DIY"
                        },
                        new
                        {
                            Id = 17,
                            Description = "Toys and games for children",
                            Name = "Toys and Games"
                        });
                });

            modelBuilder.Entity("variate.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("IdentityUserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("OrderDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Status")
                        .HasColumnType("text");

                    b.Property<decimal>("TotalCost")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("IdentityUserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("variate.Models.OrderItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("OrderId")
                        .HasColumnType("integer");

                    b.Property<int>("ProductId")
                        .HasColumnType("integer");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<int>("UnitPrice")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("variate.Models.Payment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<int>("OrderId")
                        .HasColumnType("integer");

                    b.Property<string>("PaymentDate")
                        .HasColumnType("text");

                    b.Property<string>("PaymentMethod")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("variate.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<DateOnly>("Release")
                        .HasColumnType("date");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CategoryId = 1,
                            Description = "A sleek smartphone with a powerful processor.",
                            ImageUrl = "samsung_a15_blue.jpg",
                            Name = "Samsung A15 Blue",
                            Price = 5999m,
                            Release = new DateOnly(2024, 1, 1)
                        },
                        new
                        {
                            Id = 2,
                            CategoryId = 1,
                            Description = "A high-performance laptop with a stunning display.",
                            ImageUrl = "dell_xps_15.jpg",
                            Name = "Dell XPS 15",
                            Price = 21999m,
                            Release = new DateOnly(2024, 2, 1)
                        },
                        new
                        {
                            Id = 3,
                            CategoryId = 1,
                            Description = "Noise-cancelling wireless headphones.",
                            ImageUrl = "sony_wh_1000xm4.jpg",
                            Name = "Sony WH-1000XM4",
                            Price = 3999m,
                            Release = new DateOnly(2024, 3, 1)
                        },
                        new
                        {
                            Id = 4,
                            CategoryId = 1,
                            Description = "A versatile tablet with a powerful A14 chip.",
                            ImageUrl = "ipad_air.jpg",
                            Name = "Apple iPad Air",
                            Price = 9999m,
                            Release = new DateOnly(2024, 4, 1)
                        },
                        new
                        {
                            Id = 5,
                            CategoryId = 1,
                            Description = "A full-frame mirrorless camera for professional photography.",
                            ImageUrl = "canon_eos_r6.jpg",
                            Name = "Canon EOS R6",
                            Price = 36999m,
                            Release = new DateOnly(2024, 5, 1)
                        },
                        new
                        {
                            Id = 6,
                            CategoryId = 2,
                            Description = "A healthy way to fry food with little to no oil.",
                            ImageUrl = "philips_air_fryer.jpg",
                            Name = "Philips Air Fryer",
                            Price = 2799m,
                            Release = new DateOnly(2024, 1, 1)
                        },
                        new
                        {
                            Id = 7,
                            CategoryId = 2,
                            Description = "A powerful cordless vacuum cleaner.",
                            ImageUrl = "dyson_v11.jpg",
                            Name = "Dyson V11 Vacuum Cleaner",
                            Price = 7999m,
                            Release = new DateOnly(2024, 2, 1)
                        },
                        new
                        {
                            Id = 8,
                            CategoryId = 2,
                            Description = "A versatile mixer for all your baking needs.",
                            ImageUrl = "kitchenaid_mixer.jpg",
                            Name = "KitchenAid Stand Mixer",
                            Price = 6999m,
                            Release = new DateOnly(2024, 3, 1)
                        },
                        new
                        {
                            Id = 9,
                            CategoryId = 2,
                            Description = "A convenient way to make espresso at home.",
                            ImageUrl = "nespresso_machine.jpg",
                            Name = "Nespresso Coffee Machine",
                            Price = 3499m,
                            Release = new DateOnly(2024, 4, 1)
                        },
                        new
                        {
                            Id = 10,
                            CategoryId = 2,
                            Description = "A multi-use pressure cooker for quick meals.",
                            ImageUrl = "instant_pot_duo.jpg",
                            Name = "Instant Pot Duo",
                            Price = 1599m,
                            Release = new DateOnly(2024, 5, 1)
                        },
                        new
                        {
                            Id = 11,
                            CategoryId = 3,
                            Description = "A timeless fragrance for special occasions.",
                            ImageUrl = "chanel_no_5.jpg",
                            Name = "Chanel No. 5 Perfume",
                            Price = 1999m,
                            Release = new DateOnly(2024, 1, 1)
                        },
                        new
                        {
                            Id = 12,
                            CategoryId = 3,
                            Description = "A volumizing mascara for bold lashes.",
                            ImageUrl = "maybelline_mascara.jpg",
                            Name = "Maybelline Mascara",
                            Price = 199m,
                            Release = new DateOnly(2024, 2, 1)
                        },
                        new
                        {
                            Id = 13,
                            CategoryId = 3,
                            Description = "A luxury handbag made from fine leather.",
                            ImageUrl = "gucci_handbag.jpg",
                            Name = "Gucci Handbag",
                            Price = 15999m,
                            Release = new DateOnly(2024, 3, 1)
                        },
                        new
                        {
                            Id = 14,
                            CategoryId = 3,
                            Description = "Classic sunglasses with polarized lenses.",
                            ImageUrl = "rayban_aviator.jpg",
                            Name = "Ray-Ban Aviator Sunglasses",
                            Price = 2999m,
                            Release = new DateOnly(2024, 4, 1)
                        },
                        new
                        {
                            Id = 15,
                            CategoryId = 3,
                            Description = "A long-lasting lipstick in a range of colors.",
                            ImageUrl = "mac_lipstick.jpg",
                            Name = "MAC Lipstick",
                            Price = 299m,
                            Release = new DateOnly(2024, 5, 1)
                        },
                        new
                        {
                            Id = 16,
                            CategoryId = 4,
                            Description = "A high-quality acoustic guitar for all levels.",
                            ImageUrl = "yamaha_acoustic_guitar.jpg",
                            Name = "Yamaha Acoustic Guitar",
                            Price = 4999m,
                            Release = new DateOnly(2024, 1, 1)
                        },
                        new
                        {
                            Id = 17,
                            CategoryId = 4,
                            Description = "A digital piano with realistic touch and sound.",
                            ImageUrl = "roland_digital_piano.jpg",
                            Name = "Roland Digital Piano",
                            Price = 19999m,
                            Release = new DateOnly(2024, 2, 1)
                        },
                        new
                        {
                            Id = 18,
                            CategoryId = 4,
                            Description = "A legendary electric guitar for professionals.",
                            ImageUrl = "fender_stratocaster.jpg",
                            Name = "Fender Stratocaster",
                            Price = 17999m,
                            Release = new DateOnly(2024, 3, 1)
                        },
                        new
                        {
                            Id = 19,
                            CategoryId = 4,
                            Description = "Professional cymbals for drummers.",
                            ImageUrl = "zildjian_cymbals.jpg",
                            Name = "Zildjian Cymbals",
                            Price = 5999m,
                            Release = new DateOnly(2024, 4, 1)
                        },
                        new
                        {
                            Id = 20,
                            CategoryId = 4,
                            Description = "A reliable microphone for live performances.",
                            ImageUrl = "shure_sm58.jpg",
                            Name = "Shure SM58 Microphone",
                            Price = 1999m,
                            Release = new DateOnly(2024, 5, 1)
                        },
                        new
                        {
                            Id = 21,
                            CategoryId = 5,
                            Description = "Professional-grade watercolor paints.",
                            ImageUrl = "watercolors_winsor_newton.jpg",
                            Name = "Winsor & Newton Watercolors",
                            Price = 899m,
                            Release = new DateOnly(2024, 1, 1)
                        },
                        new
                        {
                            Id = 22,
                            CategoryId = 5,
                            Description = "A set of high-quality markers for artists.",
                            ImageUrl = "copic_marker_set.jpg",
                            Name = "Copic Marker Set",
                            Price = 3999m,
                            Release = new DateOnly(2024, 2, 1)
                        },
                        new
                        {
                            Id = 23,
                            CategoryId = 5,
                            Description = "A premium sketchbook for drawing.",
                            ImageUrl = "strathmore_sketchbook.jpg",
                            Name = "Strathmore Sketchbook",
                            Price = 299m,
                            Release = new DateOnly(2024, 3, 1)
                        },
                        new
                        {
                            Id = 24,
                            CategoryId = 5,
                            Description = "Vibrant colored pencils for coloring.",
                            ImageUrl = "faber_castell_pencils.jpg",
                            Name = "Faber-Castell Colored Pencils",
                            Price = 499m,
                            Release = new DateOnly(2024, 4, 1)
                        },
                        new
                        {
                            Id = 25,
                            CategoryId = 5,
                            Description = "Oven-bake clay for sculpting projects.",
                            ImageUrl = "sculpey_clay.jpg",
                            Name = "Sculpey Clay",
                            Price = 199m,
                            Release = new DateOnly(2024, 5, 1)
                        });
                });

            modelBuilder.Entity("variate.Models.Review", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("IdentityUserId")
                        .HasColumnType("text");

                    b.Property<int>("ProductId")
                        .HasColumnType("integer");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.Property<string>("ReviewComment")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("IdentityUserId");

                    b.HasIndex("ProductId");

                    b.ToTable("Reviews");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("variate.Models.Order", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", "IdentityUser")
                        .WithMany()
                        .HasForeignKey("IdentityUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("IdentityUser");
                });

            modelBuilder.Entity("variate.Models.OrderItem", b =>
                {
                    b.HasOne("variate.Models.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("variate.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("variate.Models.Payment", b =>
                {
                    b.HasOne("variate.Models.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("variate.Models.Product", b =>
                {
                    b.HasOne("variate.Models.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                });

            modelBuilder.Entity("variate.Models.Review", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", "IdentityUser")
                        .WithMany()
                        .HasForeignKey("IdentityUserId");

                    b.HasOne("variate.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("IdentityUser");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("variate.Models.Category", b =>
                {
                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}