using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Variate.CatalogService;
using Variate.CatalogService.Models;

var builder = WebApplication.CreateBuilder(args);

var catalogConnection = builder.Configuration.GetConnectionString("CatalogDb")
    ?? "Server=sqlserver,1433;Database=variate_catalog;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True;";

builder.Services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(catalogConnection));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.EnsureCreatedAsync();
    await CatalogSeeder.SeedAsync(db);
}

var catalog = app.MapGroup("/api/v1/catalog");

catalog.MapGet("/products", async (
    CatalogDbContext db,
    string? q,
    Guid? categoryId,
    decimal? minPrice,
    decimal? maxPrice,
    bool? inStock) =>
{
    var queryable = db.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
    {
        var term = q.Trim().ToLower();
        queryable = queryable.Where(p =>
            p.Name.ToLower().Contains(term) ||
            (p.Description != null && p.Description.ToLower().Contains(term)));
    }

    if (categoryId is not null)
    {
        queryable = queryable.Where(p => p.CategoryId == categoryId.Value);
    }

    if (minPrice is not null)
    {
        queryable = queryable.Where(p => p.Price >= minPrice.Value);
    }

    if (maxPrice is not null)
    {
        queryable = queryable.Where(p => p.Price <= maxPrice.Value);
    }

    if (inStock is not null)
    {
        queryable = inStock.Value
            ? queryable.Where(p => p.StockQuantity > 0)
            : queryable.Where(p => p.StockQuantity <= 0);
    }

    var products = await queryable
        .OrderBy(p => p.Name)
        .Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Currency,
            p.StockQuantity,
            p.ImageUrl,
            p.CategoryId,
            p.Category != null ? p.Category.Name : null,
            p.CreatedAtUtc,
            p.UpdatedAtUtc))
        .ToListAsync();

    return Results.Ok(products);
});

catalog.MapGet("/products/{id:guid}", async (CatalogDbContext db, Guid id) =>
{
    var product = await db.Products
        .AsNoTracking()
        .Include(p => p.Category)
        .Where(p => p.Id == id)
        .Select(p => new ProductResponse(
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.Currency,
            p.StockQuantity,
            p.ImageUrl,
            p.CategoryId,
            p.Category != null ? p.Category.Name : null,
            p.CreatedAtUtc,
            p.UpdatedAtUtc))
        .FirstOrDefaultAsync();

    return product is null ? Results.NotFound() : Results.Ok(product);
});

catalog.MapPost("/products", async (CatalogDbContext db, UpsertProductRequest request) =>
{
    var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId);
    if (!categoryExists)
    {
        return Results.BadRequest(new { message = "Category does not exist." });
    }

    var product = new ProductEntity
    {
        Name = request.Name.Trim(),
        Description = request.Description?.Trim(),
        Price = request.Price,
        Currency = request.Currency.Trim().ToUpperInvariant(),
        StockQuantity = request.StockQuantity,
        ImageUrl = request.ImageUrl,
        CategoryId = request.CategoryId,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = DateTime.UtcNow
    };

    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/catalog/products/{product.Id}", new { product.Id });
}).RequireAuthorization();

catalog.MapPut("/products/{id:guid}", async (CatalogDbContext db, Guid id, UpsertProductRequest request) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound();
    }

    product.Name = request.Name.Trim();
    product.Description = request.Description?.Trim();
    product.Price = request.Price;
    product.Currency = request.Currency.Trim().ToUpperInvariant();
    product.StockQuantity = request.StockQuantity;
    product.ImageUrl = request.ImageUrl;
    product.CategoryId = request.CategoryId;
    product.UpdatedAtUtc = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.Ok(new { product.Id });
}).RequireAuthorization();

catalog.MapPatch("/products/{id:guid}/stock", async (CatalogDbContext db, Guid id, StockAdjustmentRequest request) =>
{
    var product = await db.Products.FindAsync(id);
    if (product is null)
    {
        return Results.NotFound();
    }

    product.StockQuantity = Math.Max(0, product.StockQuantity + request.Delta);
    product.UpdatedAtUtc = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new { product.Id, product.StockQuantity });
}).RequireAuthorization();

catalog.MapGet("/categories", async (CatalogDbContext db) =>
{
    var categories = await db.Categories.AsNoTracking()
        .OrderBy(c => c.Name)
        .Select(c => new CategoryResponse(c.Id, c.Name, c.Description))
        .ToListAsync();
    return Results.Ok(categories);
});

catalog.MapPost("/categories", async (CatalogDbContext db, UpsertCategoryRequest request) =>
{
    var exists = await db.Categories.AnyAsync(c => c.Name == request.Name.Trim());
    if (exists)
    {
        return Results.Conflict(new { message = "Category already exists." });
    }

    var category = new CategoryEntity
    {
        Name = request.Name.Trim(),
        Description = request.Description?.Trim()
    };

    db.Categories.Add(category);
    await db.SaveChangesAsync();
    return Results.Created($"/api/v1/catalog/categories/{category.Id}", new { category.Id });
}).RequireAuthorization();

catalog.MapGet("/health", () => Results.Ok(new { service = "catalog-service", status = "ok" }));

app.Run();

internal sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "variate.auth";
    public string Audience { get; init; } = "variate.services";
    public string SigningKey { get; init; } = "replace-this-in-real-deployments-with-a-long-secret";
}

internal sealed record UpsertProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    string? ImageUrl,
    Guid CategoryId);

internal sealed record StockAdjustmentRequest(int Delta);
internal sealed record UpsertCategoryRequest(string Name, string? Description);
internal sealed record CategoryResponse(Guid Id, string Name, string? Description);
internal sealed record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    int StockQuantity,
    string? ImageUrl,
    Guid CategoryId,
    string? CategoryName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

internal static class CatalogSeeder
{
    public static async Task SeedAsync(CatalogDbContext db)
    {
        if (await db.Categories.AnyAsync())
        {
            return;
        }

        var electronics = new CategoryEntity { Name = "Electronics", Description = "Devices and accessories." };
        var home = new CategoryEntity { Name = "Home", Description = "Home and kitchen essentials." };
        var fashion = new CategoryEntity { Name = "Fashion", Description = "Clothing and accessories." };
        var sports = new CategoryEntity { Name = "Sports", Description = "Outdoor and fitness." };

        db.Categories.AddRange(electronics, home, fashion, sports);
        await db.SaveChangesAsync();

        db.Products.AddRange(
            new ProductEntity
            {
                Name = "Noise Cancelling Headphones",
                Description = "Over-ear ANC headphones with 30-hour battery.",
                Price = 199.99m,
                Currency = "USD",
                StockQuantity = 25,
                CategoryId = electronics.Id,
                ImageUrl = "/img/products/headphones.jpg"
            },
            new ProductEntity
            {
                Name = "Premium Blender",
                Description = "Smart blender with preset modes.",
                Price = 149.00m,
                Currency = "USD",
                StockQuantity = 40,
                CategoryId = home.Id,
                ImageUrl = "/img/products/blender.jpg"
            },
            new ProductEntity
            {
                Name = "Performance Hoodie",
                Description = "Lightweight hoodie for all-season workouts.",
                Price = 89.50m,
                Currency = "USD",
                StockQuantity = 65,
                CategoryId = fashion.Id,
                ImageUrl = "/img/products/hoodie.jpg"
            },
            new ProductEntity
            {
                Name = "Trail Backpack",
                Description = "Water-resistant 30L daypack.",
                Price = 120.00m,
                Currency = "USD",
                StockQuantity = 18,
                CategoryId = sports.Id,
                ImageUrl = "/img/products/backpack.jpg"
            });

        await db.SaveChangesAsync();
    }
}
