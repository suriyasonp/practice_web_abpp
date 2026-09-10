using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Root Endpoint
app.MapGet("/", () => "Hello Todo API");

// Map Group
var todoGroup = app.MapGroup("/api/todos").WithTags("Todos");


// Get All Endpoint
var todos = new List<TodoGetDto>
{
    new(1, "Learn ASP.NET Core", true),
    new(2, "Build a web API", false),
    new(3, "Write unit tests", false)
};

todoGroup.MapGet("/", () => Results.Ok(todos));

// Filter todo by ID
todoGroup.MapGet("/{id}", (int id) =>
{
    var todo = todos.FirstOrDefault(x => x.Id == id);

    return todo is null
        ? Results.NotFound("Todo not found")
        : Results.Ok(todo);
});

// Create Todo
todoGroup.MapPost("/", (TodoCreateDto dto) =>
{
    var nextId = todos.Count == 0 ? 1 : todos.Max(x => x.Id) + 1;
    // prepare values to return as Get Dto.
    var todo = new TodoGetDto(nextId, dto.Title, false);
    todos.Add(todo);

    return Results.Created($"/api/todos/{todo.Id}", todo);
});

// Update Todo
todoGroup.MapPut("/{id}", (int id, TodoUpdateDto dto) =>
{
    // Find the index of the todo item with the specified ID
    var index = todos.FindIndex(x => x.Id == id);

    if (index == -1) return Results.NotFound("Todo not found");

    todos[index] = todos[index] with
    {
        Title = dto.Title,
        IsCompleted = dto.IsCompleted
    };

    return Results.Ok(todos[index]);

});

// Delete Todo
todoGroup.MapDelete("/{id}", (int id) =>
{
    var todo = todos.FirstOrDefault(x => x.Id == id);
    if(todo is null) return Results.NotFound("Todo not found");

    todos.Remove(todo);
    return Results.NoContent();
});

app.Run();
