using Microsoft.EntityFrameworkCore;
using VehicleRegistryAPI.Data;
using VehicleRegistryAPI.Repositories.Generics;
using VehicleRegistryAPI.Repositories.Implementations;
using VehicleRegistryAPI.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// implementacion data base
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// inyección de dependencias
#region Repositories 
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();
#endregion


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
