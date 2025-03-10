using EnFoco_new.Data;
using EnFoco_new.Models;
using EnFoco_new.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Text.Json;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Configurar conexión a SQL Server
var connectionString = builder.Configuration.GetConnectionString("EnFocoDB");


try
{
    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("Conexión exitosa a la base de datos.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error al conectar a la base de datos: " + ex.Message);
    // Log the exception for detailed information
    builder.Logging.AddConsole(); // Or your preferred logging provider
    var logger = builder.Build().Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Error de conexión a la base de datos");


    // Decide how to handle the connection failure.  For example:
    throw new Exception("Error de conexión a la base de datos: " + ex.Message); // Stop the app
    //Or, if you want the app to continue, even with the DB connection failure:
    //Console.WriteLine("La aplicacion continuara sin conexion a la base de datos.");
}



builder.Services.AddDbContext<EnFocoDb>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("EnFocoDB")));
builder.Services.AddScoped<NoticeService>();
builder.Services.AddLogging();
// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Index"; // Redirigir a esta página si no está autenticado
        options.LogoutPath = "/Home/Index";
        options.AccessDeniedPath = "/Home/Index";
    });

builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
var app = builder.Build();



app.UseHttpsRedirection();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
}

app.UseRouting();

// Activar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Endpoint para agregar noticias (API REST)
app.MapPost("noticias/", async (Notice n, EnFocoDb db) =>
{
    db.Notices.Add(n);
    await db.SaveChangesAsync();
    return Results.Created($"noticias/{n.Id}", n);
});

app.MapControllerRoute(
    name: "edit-route", // Nombre de la ruta (puede ser cualquiera)
    pattern: "{id}",
    defaults: new { controller = "Home", action = "Edit" } // Controlador y acción

);

//app.MapControllerRoute(
//    name: "edit-route",
//    pattern: "{id}",
//    defaults: new { controller = "Home", action = "Edit" },
//    constraints: new { httpMethod = new HttpMethodRouteConstraint("GET") }
//);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
