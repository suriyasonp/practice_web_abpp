using TodoApi.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Root Endpoint
app.MapGet("/", () => "Hello Todo API");

// Get All Endpoint
var todos = new List<TodoGetDto>
{
    new(1, "Learn ASP.NET Core", true),
    new(2, "Build a web API", false),
    new(3, "Write unit tests", false)
};

app.MapGet("/api/todos", () => Results.Ok(todos));

// Filter todo by ID
app.MapGet("/api/todos/{id}", (int id) =>
{
    var todo = todos.FirstOrDefault(x => x.Id == id);

    return todo is null
        ? Results.NotFound("Todo not found")
        : Results.Ok(todo);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
