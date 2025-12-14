using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using EventsTrackerApi.Data;
using EventsTrackerApi.Repositories;
using EventsTrackerApi.Models;
using Microsoft.OpenApi.Models;
using EventsTrackerApi.Service;
using EventsTrackerApi.Job;
using Microsoft.AspNetCore.Diagnostics;
var builder = WebApplication.CreateBuilder(args);
// Cargar User Secrets en modo Desarrollo
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}
// Configuración de base de datos MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.Parse("8.0.30")
    ));

builder.WebHost.UseUrls("http://*:5000");
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });

// Configuración de controllers y validación
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        // Forzar uso de PascalCase en JSON (creo que es el default y no es necesario). Tampoco es que fuerza el uso de PascalCase, sino que no modifica los nombres
        // y deja los nombres de las propiedades tal cual están en las clases C#, que por defecto usan PascalCase.
        // options.SerializerSettings.ContractResolver = new DefaultContractResolver();
    });
    /*.AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
     });*/

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Events Tracker API",
        Version = "v1",
        Description = "API para gestionar eventos y usuarios.",
        Contact = new OpenApiContact
        {
            Name = "Tu Nombre",
            Email = "tuemail@example.com",
            Url = new Uri("https://tu-sitio.com")
        }
    });

    // Configuración para incluir el token JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token en el formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// HttpClient factory (para inyectar IHttpClientFactory)
builder.Services.AddHttpClient();

// Registro de repositorios para inyección de dependencias
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRepository<Event>, EventRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRepository<EventTag>, EventTagRepository>();
builder.Services.AddScoped<IEventTagRepository, EventTagRepository>();
builder.Services.AddScoped<IRepository<Tag>, TagRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IRepository<Location>, LocationRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IRepository<UserDeviceToken>, DevicesRepository>();
builder.Services.AddScoped<IDevicesRepository, DevicesRepository>();
builder.Services.AddScoped<IRepository<UserImage>, UserImageRepository>();
builder.Services.AddScoped<IUserImageRepository, UserImageRepository>();

// Posts module repositories
builder.Services.AddScoped<IRepository<EventPost>, EventPostRepository>();
builder.Services.AddScoped<IEventPostRepository, EventPostRepository>();

// Invitations module repositories
builder.Services.AddScoped<IRepository<EventInvitation>, EventInvitationRepository>();
builder.Services.AddScoped<IEventInvitationRepository, EventInvitationRepository>();

// Opciones de Firebase (ProjectId y CredentialsPath)
builder.Services.Configure<FirebaseOptionsConfig>(builder.Configuration.GetSection("Firebase"));
builder.Services.Configure<NotificationsOptions>(builder.Configuration.GetSection("Notifications"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Servicio que envía a FCM
builder.Services.AddSingleton<FcmService>();
builder.Services.AddHostedService<EventsSyncJob>();
builder.Services.AddHostedService<EventsForDefeatJob>();
builder.Services.AddHostedService<InvitationNotificationJob>();

var app = builder.Build();

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// Configuración de middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
     app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Events Tracker API v1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseStaticFiles();
app.UseRouting();
app.UseHttpsRedirection();
// Middleware de manejo de excepciones global
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
 
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            // En desarrollo, muestra el error completo. En producción, un mensaje genérico.
            var errorMessage = app.Environment.IsDevelopment()
                ? $"Error: {contextFeature.Error.Message}\nStack Trace: {contextFeature.Error.StackTrace}"
                : "Ha ocurrido un error. Por favor, intente más tarde.";
 
            await context.Response.WriteAsJsonAsync(new { message = errorMessage });
        }
    });
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var error = new { message = "Ha ocurrido un error. Por favor, intente más tarde." };
        await context.Response.WriteAsJsonAsync(error);
    });
});

app.Run();
