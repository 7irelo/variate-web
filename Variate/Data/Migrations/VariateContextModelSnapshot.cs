﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Variate.Data;

#nullable disable

namespace Variate.Data.Migrations
{
    [DbContext(typeof(VariateContext))]
    partial class VariateContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("Variate.Entities.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Electronics"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Home and kitchen"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Fashion and beauty"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Toys and games"
                        },
                        new
                        {
                            Id = 5,
                            Name = "Books and DVDs"
                        },
                        new
                        {
                            Id = 6,
                            Name = "Baby Products"
                        },
                        new
                        {
                            Id = 7,
                            Name = "Outdoor and sports equipment"
                        },
                        new
                        {
                            Id = 8,
                            Name = "Health and wellness products"
                        },
                        new
                        {
                            Id = 9,
                            Name = "Arts and craft supplies"
                        },
                        new
                        {
                            Id = 10,
                            Name = "Musical Instruments"
                        });
                });

            modelBuilder.Entity("Variate.Entities.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("Release")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Variate.Entities.Product", b =>
                {
                    b.HasOne("Variate.Entities.Category", "Genre")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Genre");
                });
#pragma warning restore 612, 618
        }
    }
}
