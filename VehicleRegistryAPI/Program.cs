using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VehicleRegistryAPI.Data;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.Mappings;
using VehicleRegistryAPI.Repositories.Generics;
using VehicleRegistryAPI.Repositories.Implementations;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Tools.MIddleware;
using VehicleRegistryAPI.Tools.Validations;

var builder = WebApplication.CreateBuilder(args);


// implementacion data base
#region Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

#region Repositories 
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
//builder.Services.AddScoped<IUserRepository, UserRepository>();
#endregion

#region Services
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IPersonService, PersonService>();
// builder.Services.AddScoped<IUserService, UserService>();
#endregion

#region Validaciones
builder.Services.AddScoped<IValidator<CreatePersonDto>, CreatePersonValidator>();
builder.Services.AddScoped<IValidator<UpdatePersonDto>, UpdatePersonValidator>();
builder.Services.AddScoped<IValidator<CreateCarDto>, CreateCarValidator>();
builder.Services.AddScoped<IValidator<UpdateCarDto>, UpdateCarValidator>();

#endregion
//mapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {

        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

app.Run();
