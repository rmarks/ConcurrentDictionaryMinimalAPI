using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

var products = new ConcurrentDictionary<int,Product>();

app.MapGet("/products", () => products);

app.MapPost("/products", (CreateProductDto newProduct) => 
{
    int newId = products.Any() ? products.Count + 1 : 1;

    Product productToAdd = new() { Id = newId, Name = newProduct.Name, Stock = newProduct.Stock };

    return products.TryAdd(newId, productToAdd) 
            ? TypedResults.Created($"/products/{newId}", productToAdd) 
            : Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    { "id", new[] { "A product with this id already exists" } }
                });
});

app.MapGet("/products/{id}", (int id) =>
{
    return products.TryGetValue(id, out var product)
            ? TypedResults.Ok(product)
            : Results.NotFound();
});

app.MapPut("/products/{id}", (int id, Product product) =>
{
    if (id != product.Id) 
    {
        return Results.ValidationProblem(new Dictionary<string, string[]> 
                { 
                    { "id", new[] { "The id in the URL conflicts with the product id" } }
                });
    }
    
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