using Microsoft.EntityFrameworkCore;
using TodoApi.Dtos;
using TodoApi.Data;
using TodoApi.Models;

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

#region Todo Endpoints InMemory
// // Get All Endpoint

// var todos = new List<TodoGetDto>
// {
//     new(1, "Learn ASP.NET Core", true),
//     new(2, "Build a web API", false),
//     new(3, "Write unit tests", false)
// };

// todoGroup.MapGet("/", () => Results.Ok(todos));

// // Filter todo by ID
// todoGroup.MapGet("/{id}", (int id) =>
// {
//     var todo = todos.FirstOrDefault(x => x.Id == id);

//     return todo is null
//         ? Results.NotFound("Todo not found")
//         : Results.Ok(todo);
// });

// // Create Todo
// todoGroup.MapPost("/", (TodoCreateDto dto) =>
// {
//     var nextId = todos.Count == 0 ? 1 : todos.Max(x => x.Id) + 1;
//     // prepare values to return as Get Dto.
//     var todo = new TodoGetDto(nextId, dto.Title, false);
//     todos.Add(todo);

//     return Results.Created($"/api/todos/{todo.Id}", todo);
// });

// // Update Todo
// todoGroup.MapPut("/{id}", (int id, TodoUpdateDto dto) =>
// {
//     // Find the index of the todo item with the specified ID
//     var index = todos.FindIndex(x => x.Id == id);

//     if (index == -1) return Results.NotFound("Todo not found");

//     todos[index] = todos[index] with
//     {
//         Title = dto.Title,
//         IsCompleted = dto.IsCompleted
//     };

//     return Results.Ok(todos[index]);

// });

// // Delete Todo
// todoGroup.MapDelete("/{id}", (int id) =>
// {
//     var todo = todos.FirstOrDefault(x => x.Id == id);
//     if(todo is null) return Results.NotFound("Todo not found");

//     todos.Remove(todo);
//     return Results.NoContent();
// });

#endregion

#region Todo Endpoints With Database

todoGroup.MapGet("/", async (AppDbContext db) =>
{
    var todos = await db.Todos
        .Select(x => new TodoGetDto(x.Id, x.Title, x.IsCompleted))
        .ToListAsync();

    return Results.Ok(todos);
});

todoGroup.MapGet("/{id}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    return Results.Ok(
        new TodoGetDto(todo.Id, todo.Title, todo.IsCompleted)
    );
});

todoGroup.MapPost("/", async (TodoCreateDto dto, AppDbContext db) =>
{
    var todo = new TodoItem
    {
        Title = dto.Title,
        IsCompleted = false,
        CreatedAt = DateTime.UtcNow
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    var result = new TodoGetDto(todo.Id, todo.Title, todo.IsCompleted);
    return Results.Created($"/api/todos/{todo.Id}", result);
});

todoGroup.MapPut("/{id}", async (int id, TodoUpdateDto dto, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Title = dto.Title;
    todo.IsCompleted = dto.IsCompleted;
    await db.SaveChangesAsync();

    return Results.Ok(
        new TodoGetDto(todo.Id, todo.Title, todo.IsCompleted)
    );
});

todoGroup.MapDelete("/{id}", async (int id, AppDbContext db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    db.Todos.Remove(todo);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

#endregion

app.Run();
