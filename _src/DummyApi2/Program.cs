var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Dummy API 2 - Product Service endpoints
app.MapGet("/api/products", () => new[]
{
    new { Id = 1, Name = "Laptop", Price = 1299.99m, Category = "Electronics", Stock = 15 },
    new { Id = 2, Name = "Mouse", Price = 29.99m, Category = "Electronics", Stock = 50 },
    new { Id = 3, Name = "Keyboard", Price = 79.99m, Category = "Electronics", Stock = 25 },
    new { Id = 4, Name = "Monitor", Price = 299.99m, Category = "Electronics", Stock = 8 }
})
.WithName("GetProducts")
.WithSummary("Get all products")
.WithOpenApi();

app.MapGet("/api/products/{id}", (int id) => 
{
    var products = new[]
    {
        new { Id = 1, Name = "Laptop", Price = 1299.99m, Category = "Electronics", Stock = 15, Description = "High-performance laptop" },
        new { Id = 2, Name = "Mouse", Price = 29.99m, Category = "Electronics", Stock = 50, Description = "Wireless optical mouse" },
        new { Id = 3, Name = "Keyboard", Price = 79.99m, Category = "Electronics", Stock = 25, Description = "Mechanical gaming keyboard" },
        new { Id = 4, Name = "Monitor", Price = 299.99m, Category = "Electronics", Stock = 8, Description = "24-inch LED monitor" }
    };
    
    var product = products.FirstOrDefault(p => p.Id == id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
})
.WithName("GetProductById")
.WithSummary("Get product by ID")
.WithOpenApi();

app.MapPost("/api/products", (object product) => Results.Created($"/api/products/5", new { Id = 5, Message = "Product created successfully" }))
.WithName("CreateProduct")
.WithSummary("Create a new product")
.WithOpenApi();

app.MapGet("/api/categories", () => new[]
{
    new { Id = 1, Name = "Electronics", Count = 4 },
    new { Id = 2, Name = "Clothing", Count = 0 },
    new { Id = 3, Name = "Books", Count = 0 }
})
.WithName("GetCategories")
.WithSummary("Get all categories")
.WithOpenApi();

app.MapGet("/health", () => new { Status = "Healthy", Service = "DummyApi2-ProductService", Timestamp = DateTime.UtcNow })
.WithName("HealthCheck")
.WithOpenApi();

app.MapDefaultEndpoints();

app.Run();
