using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
using BracketBuilder.Auth;
using BracketBuilder.Database;
using BracketBuilder.Services;
using BracketBuilder.Services.Interfaces;
using BlazorPro.BlazorSize;
using BracketBuilder.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


// Load salt from appsettings.json
var configuration = builder.Configuration;
var salt = configuration["Salt"];

UserAccount.Salt=salt;
//FieldInfo fieldInfo = typeof(UserAccount).GetField("Salt", BindingFlags.Public | BindingFlags.Static);
//if (fieldInfo != null)
//{
//    fieldInfo.SetValue(null, salt);
//}

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

// Add services to the container.
builder.Services.AddAuthenticationCore();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<ProtectedSessionStorage>();

//save dark mode prefference
builder.Services.AddScoped<IUserPreferencesService,UserPreferencesService>();

//Refresh service
builder.Services.AddScoped<RemoteService>();
//builder.Services.AddScoped<ThemeCallbackService>();

builder.Services.AddSingleton<CommunicationService>();

builder.Services.AddMediaQueryService();

//database
builder.Services.AddHttpClient();
builder.Services.AddSqlite<DatabaseContext>("Data Source=Database\\data\\bracketBuilder.db");
//builder.Services.AddDbContext<DatabaseContext>(
//               o => o.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

//auth
//i changed this from singleton to scoped (i hope it wont crash in the future because of conflicts or whatever)
builder.Services.AddScoped<IDatabaseService, DatabaseService>();
builder.Services.AddScoped<DatabaseDTOService>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddMudServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

// Initialize the database
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var AccountDB = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    if (AccountDB.Database.EnsureCreated())
    {
        DefaultData.Initialize(AccountDB);
    }
}


app.Run();