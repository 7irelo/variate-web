using Variate.Entities;
using Variate.Dtos;
using Variate.Data;
namespace Variate.Endpoints;

public static class ProductsEndpoints
{
    private static readonly List<ProductDto> products = [
        new (1, "After Hours", "Dark Synth Pop", 599.99M, new DateOnly(2020, 7, 9)),
        new (2, "Blonde.", "Rhythm and Blues", 499.99M, new DateOnly(2017, 8, 22)),
        new (3, "Good Kid M.A.A.D City", "West Coast Rap", 699.99M, new DateOnly(2017, 3, 29))
    ];

    public static RouteGroupBuilder MapProductsEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("products").WithParameterValidation();

        const string GetProductEndpointName = "GetProduct";

        group.MapGet("/", () => products);

        group.MapGet("/{id}", (int id) => 
        {
            ProductDto? product = products.Find(product => product.Id == id);

            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName(GetProductEndpointName);

        group.MapPost("/", (CreateProductDto newProduct, VariateContext dbContext) => {
            Product product = new()
            {
                Name = newProduct.Name,
                Category = dbContext.Categories.Find(newProduct.CategoryId),
                CategoryId = newProduct.CategoryId,
                Price = newProduct.Price,
                Release = newProduct.Release
            };

            dbContext.Products.Add(product);
            dbContext.SaveChanges();

            ProductDto productDto = new(
                product.Id,
                product.Name,
                product.Category!.Name,
                product.Price,
                product.Release
            );

            return Results.CreatedAtRoute(GetProductEndpointName, new {id = product.Id}, productDto);
        });

        group.MapPut("/{id}", (int id, UpdateProductDto updatedProduct) => 
        {
            int index = products.FindIndex(product => product.Id == id);

            if (index == -1)
            {
                return Results.NotFound();
            }

            products[index] = new ProductDto(
                id, 
                updatedProduct.Name, 
                updatedProduct.Category, 
                updatedProduct.Price, 
                updatedProduct.Release);

            return Results.NoContent();
        });


        group.MapDelete("/{id}", (int id) => 
        {
            products.RemoveAll(product => product.Id == id);

            return Results.NoContent();
        });

        return group;

    }
}