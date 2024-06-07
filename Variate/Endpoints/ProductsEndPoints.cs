using Variate.Dtos;

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

        group.MapPost("/", (CreateProductDto newProduct) => {
            ProductDto product = new(
                products.Count + 1, 
                newProduct.Name, 
                newProduct.Genre, 
                newProduct.Price, 
                newProduct.Release);
            products.Add(product);

            return Results.CreatedAtRoute(GetProductEndpointName, new {id = product.Id}, product);
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
                updatedProduct.Genre, 
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