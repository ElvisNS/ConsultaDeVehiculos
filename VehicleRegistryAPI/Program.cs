using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using VehicleRegistryAPI.Configurations;
using VehicleRegistryAPI.Data;
using VehicleRegistryAPI.Data.Interceptors;
using VehicleRegistryAPI.DTOS.Auth;
using VehicleRegistryAPI.DTOS.Cars;
using VehicleRegistryAPI.DTOS.Persons;
using VehicleRegistryAPI.DTOS.Users;
using VehicleRegistryAPI.Mappings;
using VehicleRegistryAPI.Repositories.Generics;
using VehicleRegistryAPI.Repositories.Implementations;
using VehicleRegistryAPI.Repositories.Interfaces;
using VehicleRegistryAPI.Services.Auth;
using VehicleRegistryAPI.Services.Car;
using VehicleRegistryAPI.Services.Person;
using VehicleRegistryAPI.Services.Roles;
using VehicleRegistryAPI.Services.Users;
using VehicleRegistryAPI.Tools.MIddleware;
using VehicleRegistryAPI.Tools.Security;
using VehicleRegistryAPI.Tools.Validations.AuthValidations;
using VehicleRegistryAPI.Tools.Validations.CarValidations;
using VehicleRegistryAPI.Tools.Validations.PersonValidations;
using VehicleRegistryAPI.Tools.Validations.UserValidations;
using VehicleRegistryAPI.Tools.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

#region JwtSettings 

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.Key))
{
    throw new InvalidOperationException("JwtSettings no está configurado correctamente en appsettings.json");
}

// Validar longitud de la clave (mínimo 32 caracteres para HMAC-SHA256)
if (jwtSettings.Key.Length < 32)
{
    throw new InvalidOperationException("JwtSettings.Key debe tener al menos 32 caracteres (256 bits) para seguridad adecuada.");
}

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// ===== 2. Configurar autenticación JWT Bearer =====
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();

            // Obtener logger para auditoría
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Intento de acceso no autorizado a {Path}. TraceId: {TraceId}",
                context.HttpContext.Request.Path, context.HttpContext.TraceIdentifier);

            var problem = ProblemDetailsHelper.Create(
                context.HttpContext,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "No estás autenticado o el token es inválido"
            );

            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(problem);
        },

        OnForbidden = async context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Acceso prohibido a {Path} para el usuario {User}. TraceId: {TraceId}",
                context.HttpContext.Request.Path,
                context.HttpContext.User?.Identity?.Name ?? "unknown",
                context.HttpContext.TraceIdentifier);

            var problem = ProblemDetailsHelper.Create(
                context.HttpContext,
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "No tienes permisos para acceder a este recurso"
            );

            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    };
});
#endregion

#region Database
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
{
    var interceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(interceptor); // Aquí se añade el interceptor
});
#endregion

#region Repositories 
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();
#endregion

#region Services
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
#endregion

#region Validaciones
builder.Services.AddScoped<IValidator<CreatePersonDto>, CreatePersonValidator>();
builder.Services.AddScoped<IValidator<UpdatePersonDto>, UpdatePersonValidator>();
builder.Services.AddScoped<IValidator<CreateCarDto>, CreateCarValidator>();
builder.Services.AddScoped<IValidator<UpdateCarDto>, UpdateCarValidator>();
builder.Services.AddScoped<IValidator<LoginDto>, AuthValidator>();
builder.Services.AddScoped<IValidator<CreateUserDto>, CreateUserValidator>();
builder.Services.AddScoped<IValidator<UpdateUserDto>, UpdateUserValidator>();
#endregion


builder.Services.AddScoped<AuditableEntityInterceptor>();
//mapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "VehicleRegistryAPI", Version = "v1" });

    // Configuración JWT Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Ingrese el token JWT con el prefijo 'Bearer '",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
