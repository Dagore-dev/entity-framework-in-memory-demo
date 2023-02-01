using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";

builder.Services.AddSqlite<PizzaDb>(connectionString);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config => {
  config.SwaggerDoc("v1", new OpenApiInfo {
    Title = "PizzaStore API",
    Description = "making the Pizzas you love",
    Version = "v1"
  });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(config => {
  config.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

app.MapGet("/", () => "Hello World!");

app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());

app.MapPost("/pizzas", async (PizzaDb db, Pizza pizza) => {
  await db.Pizzas.AddAsync(pizza);
  await db.SaveChangesAsync();
  return Results.Created($"/pizza/{pizza.Id}", pizza);
});

app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));

app.MapPut("/pizza/{id}", async (PizzaDb db, Pizza updatedPizza, int id) => {
  Pizza? pizza = await db.Pizzas.FindAsync(id);

  if (pizza is null)
  {
    return Results.NotFound();
  }

  pizza.Name = updatedPizza.Name;
  pizza.Description = updatedPizza.Description;

  await db.SaveChangesAsync();
  return Results.NoContent();
});

app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) => {
  Pizza? pizza = await db.Pizzas.FindAsync(id);

  if (pizza is null)
  {
    return Results.NotFound();
  }

  db.Pizzas.Remove(pizza);
  await db.SaveChangesAsync();
  return Results.Ok();
});

app.Run();
