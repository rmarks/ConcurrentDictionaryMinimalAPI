using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var products = new ConcurrentDictionary<int,Product>();

app.MapGet("/products", () => products);

app.MapPost("/products", (CreateProductDto newProduct) => 
{
    int newId = products.Any() ? products.Count + 1 : 1;

    Product productToAdd = new() { Id = newId, Name = newProduct.Name, Stock = newProduct.Stock };

    return products.TryAdd(newId, productToAdd) 
            ? Results.Created($"/products/{newId}", productToAdd) 
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
    if (id != product.Id) return Results.BadRequest();

    if (!products.TryGetValue(id, out _)) return Results.NotFound();

    products[id] = product;

    return Results.NoContent();
});

app.MapDelete("/products/{id}", (int id) => 
{
    return products.TryRemove(id, out _)
            ? Results.NoContent()
            : Results.NotFound();
});

app.Run();

class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Stock { get; set; }
}

record CreateProductDto(string Name, int Stock);