using LibraryWebApplication1.Models;
using LibraryWebApplication1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Azure.Messaging.ServiceBus;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<DblibraryContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<DblibraryContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = "Application";
    o.DefaultSignInScheme = "External";
})
            .AddCookie("Application")
            .AddCookie("External")
            .AddGoogle(o =>
            {
                o.ClientId = "755861786405-vp3j3666mmhr8l3outj8lm70jnu1bjh3.apps.googleusercontent.com";
                o.ClientSecret = "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";
            });
builder.Services.AddSingleton<ServiceBusClient>(provider =>
{
    string connectionString = builder.Configuration.GetConnectionString("ServiceBusConnection");
    return new ServiceBusClient(connectionString);
});
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<DblibraryContext>();
        var users = context.Users.ToList();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();

