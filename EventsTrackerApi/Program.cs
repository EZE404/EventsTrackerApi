using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyProject.Services;
using MyProject.Repositories;
using MyProject.Helpers;
using FluentValidation.AspNetCore;
using Newtonsoft.Json;
using MyProject.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuración de base de datos MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

// Configuración de JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Configuración de inyección de dependencias
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEmailService, EmailService>();
// Registro de repositorios para inyección de dependencias
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
// Añadir más repositorios según sea necesario

// Configuración de MailKit
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

// Configuración de controllers y validación
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore)
    .AddFluentValidation(config =>
        config.RegisterValidatorsFromAssemblyContaining<Program>());

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configuración de middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
