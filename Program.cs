using EnFoco_new.Data;
using EnFoco_new.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using NSwag;
using NSwag.Generation.AspNetCore;




var builder = WebApplication.CreateBuilder(args);

// Cadena de conexi�n
var connectionString = builder.Configuration.GetConnectionString("EnFocoDB");

// Registrar DbContext con soporte para Identity
builder.Services.AddDbContext<EnFocoDb>(options =>
    options.UseSqlServer(connectionString));

// Agregar Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<EnFocoDb>()
    .AddDefaultTokenProviders();

// Configuraci�n de JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // �Ojo en producci�n!
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Registrar servicios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INoticeService, NoticeService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();

// Configurar CORS para que Blazor WASM pueda consumir la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5202") // Exacto
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Importante si manejas cookies
    });
});

// Solo agregar controladores API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddOpenApiDocument(config =>
{
    config.Title = "EnFoco API";
    config.Version = "v1";
    config.Description = "API para la gesti�n de noticias de EnFoco";

    // Definici�n del esquema de seguridad para JWT
    config.AddSecurity("JWT", new NSwag.OpenApiSecurityScheme
    {
        Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
        Description = "Introduce el token JWT como: Bearer {token}"
    });

    // Requisito de seguridad: aplicar el esquema a todas las operaciones
    config.OperationProcessors.Add(
        new NSwag.Generation.Processors.Security.AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

var app = builder.Build();

//app.UseHttpsRedirection();

// Usar CORS con la pol�tica definida
app.UseCors("AllowBlazorClient");

app.UseAuthentication(); // �Aseg�rate de que UseAuthentication venga antes de UseAuthorization!
app.UseAuthorization();

app.UseOpenApi();
app.UseSwaggerUi();
app.UseReDoc();

// Mapear controladores API
app.MapControllers();

app.UseStaticFiles();

app.Run();