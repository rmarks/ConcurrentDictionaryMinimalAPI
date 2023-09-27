using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var products = new ConcurrentDictionary<int,Product>();

app.MapGet("/products", () => products);

app.MapPost("/products", (Product product) => 
{
    product.Id = products.Any() ? products.Count + 1 : 1;

    return products.TryAdd(product.Id, product) 
            ? Results.Created("/products/{product.Id}", product) 
            : Results.BadRequest();
});

app.MapGet("/products/{id}", (int id) =>
{
    return products.TryGetValue(id, out var product)
            ? Results.Ok(product)
            : Results.NotFound();
});

app.MapPut("/products/{id}", (int id, Product product) =>
{
    if (!products.TryGetValue(id, out var productToUpdate)) return Results.NotFound();

    products[id] = product;

    return Results.NoContent();
});

app.MapDelete("/products/{id}", (int id) => 
{
    return products.TryRemove(id, out _)
            ? Results.NoContent()
            : Results.BadRequest();
});

app.Run();

class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
}