using GovConnect.Data;
using GovConnect.Models;
using GovConnect.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<CitizenService>();
var connectionString = builder.Configuration.GetConnectionString("SQLServerConnection") ?? throw new InvalidOperationException("Connection string 'SQLServerIdentityConnection' not found.");
builder.Services.AddDbContext<SqlServerDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddIdentity<Citizen, IdentityRole>(
    options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 4;
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ ";
        //options.Tokens.ProviderMap.Add("Default", new TokenProviderDescriptor(typeof(DataProtectorTokenProvider<Citizen>)));
    })
    .AddEntityFrameworkStores<SqlServerDbContext>().AddDefaultTokenProviders();
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

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
