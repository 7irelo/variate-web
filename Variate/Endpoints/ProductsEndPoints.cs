using Variate.Entities;
using Variate.Dtos;
using Variate.Data;
using Variate.Mapping;
using Microsoft.EntityFrameworkCore;
namespace Variate.Endpoints;

public static class ProductsEndpoints
{
    public static RouteGroupBuilder MapProductsEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("products").WithParameterValidation();

        const string GetProductEndpointName = "GetProduct";

        group.MapGet("/", (VariateContext dbContext) => 
            dbContext.Products.Include(product => product.Category)
            .Select(product => product.ToProductSummaryDto())
            .AsNoTracking());

        group.MapGet("/{id}", (int id, VariateContext dbContext) => 
        {
            Product? product = dbContext.Products.Find(id);

            return product is null ? Results.NotFound() : Results.Ok(product.ToProductDetailsDto());
        })
        .WithName(GetProductEndpointName);

        group.MapPost("/", (CreateProductDto newProduct, VariateContext dbContext) => {
            Product product = newProduct.ToEntity();
            // product.Category = dbContext.Categories.Find(newProduct.CategoryId);

            dbContext.Products.Add(product);
            dbContext.SaveChanges();

            return Results.CreatedAtRoute(GetProductEndpointName, new {id = product.Id}, product.ToProductDetailsDto());
        });

        group.MapPut("/{id}", (int id, UpdateProductDto updatedProduct, VariateContext dbContext) => 
        {
            var existingProduct = dbContext.Products.Find(id);

            if (existingProduct is null)
            {
                return Results.NotFound();
            }

            dbContext.Entry(existingProduct).CurrentValues.SetValues(updatedProduct.ToEntity(id));
            dbContext.SaveChanges();

            return Results.NoContent();
        });


        group.MapDelete("/{id}", (int id, VariateContext dbContext) => 
        {
            dbContext.Products.Where(product => product.Id == id)
                     .ExecuteDelete();

            return Results.NoContent();
        });

        return group;

    }
}