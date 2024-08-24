using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace variate.Migrations
{
    /// <inheritdoc />
    public partial class Products : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "Name", "Price", "Release" },
                values: new object[,]
                {
                    { 1, 1, "A modern smartphone with all the latest features.", "smartphone.jpg", "Smartphone", 5999m, new DateOnly(2024, 1, 1) },
                    { 2, 1, "A high-performance laptop for gaming and work.", "laptop.jpg", "Laptop", 14999m, new DateOnly(2024, 2, 1) },
                    { 3, 2, "A powerful blender for making smoothies and more.", "blender.jpg", "Blender", 1299m, new DateOnly(2024, 3, 1) },
                    { 4, 2, "A compact microwave with multiple cooking functions.", "microwave.jpg", "Microwave", 1999m, new DateOnly(2024, 4, 1) },
                    { 5, 3, "A long-lasting lipstick available in various shades.", "lipstick.jpg", "Lipstick", 199m, new DateOnly(2024, 5, 1) },
                    { 6, 3, "A luxurious fragrance for special occasions.", "perfume.jpg", "Perfume", 799m, new DateOnly(2024, 6, 1) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);
        }
    }
}
