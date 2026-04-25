using Drippin.Data;
using Drippin.Models;
using Drippin.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// Registra el DrippinContext y le dice cómo conectarse a la base de datos SQL Server usando la DefaultConnection de appsettings.json.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DrippinContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Añade los servicios para que funcione la arquitectura MVC.
builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) /* Esto define el sistema de cookies como metodo de autenticación*/
    
    /* Configura como se comportan las cookies. */
    .AddCookie(options =>
    {
        options.LoginPath = "/Accesos/Login"; /* Si un usuario no autenticado intenta acceder a una página protegida, lo manda al login */

        options.AccessDeniedPath = "/Inicio/AccesoDenegado"; /* Si el usuario registrado intenta acceder a una página para la que no tiene permisos,
                                                                lo manda a la página de acceso denegado. */

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); /* Tiempo de expiración de la cookie POR DEFECTO. */

        options.SlidingExpiration = true; /* el contador de 30 dias se reinicia cada vez que el usuario visita el sitio. */
    });

builder.Services.AddAuthorization(); /* Servicio de autorización */


/* Busca la sección EmailSettings de appsettings para rellenar un objeto EmailSettings (los vincula) */
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings")); 

builder.Services.AddTransient<IEmailService, EmailService>(); /* Registra el servicio de Email. */

builder.Services.AddDistributedMemoryCache(); // Servicio de almacenamiento temporal de la sesión
builder.Services.AddSession(options => // Configuración de la sesión
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de inactividad

    /* Configuraciones de seguridad para la cookie de sesion */
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build(); /* construye la aplicación usando los servicios definidos anteriormente */

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection(); // redirige cualquier peticion http a https
app.UseStaticFiles(); // permite al sitio usar archivos estáticos como CSS, JavaScript e imagenes desde wwwroot

app.UseSession(); // Activa el middleware de sesión configurado anteriormente

app.UseRouting(); // Mira la URL y decide qué controlador y qué metodo la manejan.

app.UseAuthentication(); // Lee la cookie de autenticación y establece quien es el usuario.
app.UseAuthorization(); // Comprueba los permisos del usuario 


/* Define la ruta por defecto de las peticiones MVC: controlador, acción y parámetro opcional id */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // define la estructura de las urls

app.Run(); // Enciende el servidor para escuchar las peticiones de los usuarios.
