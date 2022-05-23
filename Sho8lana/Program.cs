using exp.Services;
using Hangfire;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Sho8lana.Areas.Identity.Pages.Account;
using Sho8lana.Data;
using Sho8lana.Hubs;
using Sho8lana.Models;
using Sho8lana.Services;
using Sho8lana.Unit_Of_Work;
using System.Text.Json.Serialization;
using Twilio;

var builder = WebApplication.CreateBuilder(args);






// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("SmarterAsp");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options
    .UseLazyLoadingProxies()
    .UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<Customer, IdentityRole>(
    options => options.SignIn.RequireConfirmedAccount = true
    )
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

builder.Services.AddSignalR();
builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();
//builder.Services.AddControllers()
//        .AddNewtonsoftJson(x =>x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);


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
services.Configure<TwilioVerifySettings>(builder.Configuration.GetSection("Twilio"));
var accountSid = builder.Configuration["Twilio:AccountSID"];
var authToken = builder.Configuration["Twilio:AuthToken"];
TwilioClient.Init(accountSid, authToken);
builder.Services.AddTransient<IEmailSender, EmailSender>();
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
app.UseHangfireDashboard(pathMatch:"/hangfire");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

app.Run();
