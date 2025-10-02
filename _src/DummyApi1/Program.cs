var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Dummy API 1 - User Service endpoints
app.MapGet("/api/users", () => new[]
{
    new { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Department = "Engineering" },
    new { Id = 2, Name = "Bob Smith", Email = "bob@example.com", Department = "Marketing" },
    new { Id = 3, Name = "Carol Davis", Email = "carol@example.com", Department = "Sales" }
})
.WithName("GetUsers")
.WithSummary("Get all users")
.WithOpenApi();

app.MapGet("/api/users/{id}", (int id) => 
{
    var users = new[]
    {
        new { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Department = "Engineering", Status = "Active" },
        new { Id = 2, Name = "Bob Smith", Email = "bob@example.com", Department = "Marketing", Status = "Active" },
        new { Id = 3, Name = "Carol Davis", Email = "carol@example.com", Department = "Sales", Status = "Inactive" }
    };
    
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
})
.WithName("GetUserById")
.WithSummary("Get user by ID")
.WithOpenApi();

app.MapPost("/api/users", (object user) => Results.Created($"/api/users/4", new { Id = 4, Message = "User created successfully" }))
.WithName("CreateUser")
.WithSummary("Create a new user")
.WithOpenApi();

app.MapGet("/health", () => new { Status = "Healthy", Service = "DummyApi1-UserService", Timestamp = DateTime.UtcNow })
.WithName("HealthCheck")
.WithOpenApi();

app.MapDefaultEndpoints();

app.Run();
