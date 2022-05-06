using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Serialization;
using Sho8lana.Data;
using Sho8lana.Hubs;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);






// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseLazyLoadingProxies().UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<Customer,IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

builder.Services.AddSignalR();

builder.Services.AddMvc().AddJsonOptions(options =>
options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
var services = builder.Services;
//var configuration=builder.Configuration;


services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)  
       .AddGoogle(options =>
       {

           options.ClientId = "142484325254-saqqcaksdr0tjh22pb17m7lqu4joovno.apps.googleusercontent.com";
           options.ClientSecret = "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";

       }).AddFacebook(options =>
       {

           options.ClientId = "718694602584796";
           options.ClientSecret = "482adbb890fb5e901a5603f6b22a36d5";

       });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.Run();
