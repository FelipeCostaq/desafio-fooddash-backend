using FoodDash.Data;
using FoodDash.DTO;
using FoodDash.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "FoodDash v1"));
}

// Menu
app.MapGet("/menu", async ([FromQuery] string? status, [FromServices] AppDbContext db) =>
{
    var menu = status?.ToLower() == "available" ? db.Menu.Where(i => i.IsAvailable).AsNoTracking() :
               status?.ToLower() == "unavailable" ? db.Menu.Where(i => !i.IsAvailable).AsNoTracking() :
               db.Menu.AsNoTracking();

    return Results.Ok(await menu.ToListAsync());
});

app.MapPost("/menu", async (MenuDTO menuDTO, AppDbContext db) =>
{
    if (!Enum.IsDefined(typeof(FoodCategory), menuDTO.Category))
    {
        return Results.BadRequest("Categoria inválida. Escolha entre Lanche (0), Bebida (1) ou Sobremesa (2).");
    }

    Menu menu = new Menu();

    menu.Title = menuDTO.Title;
    menu.Description = menuDTO.Description;
    menu.Price = menuDTO.Price;
    menu.Category = menuDTO.Category;
    menu.IsAvailable = menuDTO.IsAvailable;

    await db.Menu.AddAsync(menu);
    await db.SaveChangesAsync();
    return Results.Created($"/menu/{menu.Id}", menu);
});

app.MapPut("/menu/{id:int}", async (int id, MenuEditDTO menuEditDto, AppDbContext db) =>
{
    var foundItem = await db.Menu.FindAsync(id);

    if (foundItem is null)
        return Results.NotFound();

    if (menuEditDto.Price < 0.01M)
        return Results.BadRequest("O Preço deve ser igual ou superior a R$0,01");

    if (!Enum.IsDefined(typeof(FoodCategory), menuEditDto.Category))
    {
        return Results.BadRequest("Categoria inválida. Escolha entre Lanche (0), Bebida (1) ou Sobremesa (2).");
    }

    foundItem.Title = menuEditDto.Title;
    foundItem.Description = menuEditDto.Description;
    foundItem.Price = menuEditDto.Price;
    foundItem.Category = menuEditDto.Category;

    await db.SaveChangesAsync();

    return Results.Ok(foundItem);
});

app.MapPatch("/menu/{id:int}/availability", async (int id, MenuAvailabilityDTO menuAvailability, AppDbContext db) =>
{
    var foundItem = await db.Menu.FindAsync(id);
    
    if (foundItem is null)
        return Results.NotFound();

    if (foundItem.IsAvailable == menuAvailability.IsAvailable)
        return Results.Conflict($"Esse item já está com status {foundItem.IsAvailable}");

    foundItem.IsAvailable = menuAvailability.IsAvailable;

    await db.SaveChangesAsync();

    return Results.Ok(foundItem);
});

app.MapDelete("/menu/{id:int}", async (int id, AppDbContext db) =>
{
    var foundItem = await db.Menu.FindAsync(id);

    if (foundItem is null)
        return Results.NotFound();

    db.Menu.Remove(foundItem);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
