using DemoMinimalAPI.Data;
using DemoMinimalAPI.Models;
using Microsoft.EntityFrameworkCore;
using MiniValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MinimalContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/supplier", async (MinimalContextDb context) =>
    await context.Suppliers.ToListAsync())
    .WithName("GetSuppliers")
    .WithTags("Supplier");

app.MapGet("/supplier/{id}", async (Guid id, MinimalContextDb context) =>
    await context.Suppliers.FindAsync(id)
        is Supplier supplier
            ? Results.Ok(supplier)
            : Results.NotFound())
    .Produces<Supplier>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("GetSupplierById")
    .WithTags("Supplier");

app.MapPost("/supplier", async (MinimalContextDb context, Supplier supplier) =>
{
    if (!MiniValidator.TryValidate(supplier, out var errors))
        return Results.ValidationProblem(errors);
    context.Suppliers.Add(supplier);
    var result = await context.SaveChangesAsync(); 

    return result > 0
        //Results.Created($"/supplier/{supplier.Id}", supplier);
        ?Results.CreatedAtRoute("GetSuppliersById", new {id = supplier.Id }, supplier)
        :Results.BadRequest("There was a problem saving the registry");
}).ProducesValidationProblem()
.Produces<Supplier>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.WithName("PostSupplier")
.WithTags("Supplier");

app.Run();
