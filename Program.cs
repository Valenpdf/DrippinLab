using Drippin.Data;
using Drippin.Models;
using Drippin.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DrippinContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) /* Esto define el login como OBLIGATORIO para que se establezcan
                                                                                       * las cookies*/
    .AddCookie(options =>
    {
        options.LoginPath = "/Accesos/Login";
        options.AccessDeniedPath = "/Accesos/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); /* Tiempo de expiración de la cookie.  */
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddDistributedMemoryCache(); // Servicio de almacenamiento de la sesión
builder.Services.AddSession(options => // Configuración de la sesión
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de inactividad
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


/* Define la rutap or defecto de las peticiones MVC: controlador, acción y parámetro opcional id */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
