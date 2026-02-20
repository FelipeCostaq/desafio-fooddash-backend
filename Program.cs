using FoodDash.Data;
using FoodDash.DTO;
using FoodDash.Enums;
using FoodDash.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions> (options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

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
}).WithTags("Menu");

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
}).WithTags("Menu");

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
}).WithTags("Menu");

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
}).WithTags("Menu");

app.MapDelete("/menu/{id:int}", async (int id, AppDbContext db) =>
{
    var foundItem = await db.Menu.FindAsync(id);

    if (foundItem is null)
        return Results.NotFound();

    db.Menu.Remove(foundItem);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).WithTags("Menu");

// Orders
app.MapGet("/orders", async ([FromQuery] string? status, AppDbContext db) =>
{
    var query = db.Orders
    .Include(o => o.Items)
    .ThenInclude(i => i.Product)
    .AsNoTracking();

    query = status?.ToLower() switch
    {
        "received" => query.Where(i => i.Status == OrderStatus.Recebido),
        "preparing" => query.Where(i => i.Status == OrderStatus.Preparando),
        "route" => query.Where(i => i.Status == OrderStatus.EmRota),
        "delivered" => query.Where(i => i.Status == OrderStatus.Entregue),
        _ => query 
    };

    var orders = await query.ToListAsync();

    return Results.Ok(orders);
}).WithTags("Orders");

app.MapGet("/orders/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var foundOrder = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Product).AsTracking().FirstOrDefaultAsync(j => j.Id == id);

    if (foundOrder is null)
        return Results.NotFound();

    return Results.Ok(foundOrder);
}).WithTags("Orders");

app.MapPost("/orders", async (OrdersDTO ordersDTO, AppDbContext db) =>
{
    var order = new Orders
    {
        Id = Guid.CreateVersion7(),
        CustomerName = ordersDTO.CustomerName,
        Address = ordersDTO.Address,
        Status = OrderStatus.Recebido,
        TotalPrice = 0
    };

    foreach (var itemDTO in ordersDTO.Items)
    {
        var product = await db.Menu.FindAsync(itemDTO.ProductId);

        if (product is null)
            return Results.BadRequest($"Produto {product.Id} não encontrado.");

        var orderItem = new OrderItem
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = itemDTO.Quantity,
            UnitPrice = product.Price
        };

        order.TotalPrice += (orderItem.UnitPrice * orderItem.Quantity);

        order.Items.Add(orderItem);
    }

    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Created($"/orders/{order.Id}", order);
}).WithTags("Orders");

app.MapPatch("/orders/{id:guid}/status", async (Guid id, OrdersStatusDTO ordersStatusDTO, AppDbContext db) =>
{
    var foundOrder = await db.Orders.FindAsync(id);

    if (foundOrder is null)
        return Results.NotFound();

    if (!Enum.IsDefined(typeof(OrderStatus), ordersStatusDTO.Status))
        return Results.BadRequest("Status inválido. Escolha entre Recebido (0), Preparando (1), EmRota (2), Entregue (3) ou Cancelado (4).");

    if (foundOrder.Status == OrderStatus.Entregue)
        return Results.Conflict("Esse pedido esta com status Entregue e não pode ter seu status alterado.");

    if (foundOrder.Status == OrderStatus.Cancelado)
        return Results.Conflict("Esse pedido esta com status Cancelado e não pode ter seu status alterado.");

    if ((int)ordersStatusDTO.Status < (int)foundOrder.Status)
        return Results.Conflict($"Esse pedido esta com status {foundOrder.Status} e não pode ter voltar para um status anterior.");

    foundOrder.Status = ordersStatusDTO.Status;

    await db.SaveChangesAsync();
    return Results.Ok(foundOrder);
}).WithTags("Orders");

app.MapDelete("/orders/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var foundOrder = await db.Orders.FindAsync(id);

    if (foundOrder is null)
        return Results.NotFound();

    db.Remove(foundOrder);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).WithTags("Orders");

app.Run();
