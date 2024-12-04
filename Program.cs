using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ISchemeRepository<Scheme>,SchemeRepository>();
builder.Services.AddScoped<IServiceRepository<Service>, ServiceRepository>();
builder.Services.AddScoped<IGrievanceRepository<Grievance>, GrievanceRepository>();
builder.Services.AddScoped<ISchemeService, SchemeService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IGrievanceService, GrievanceService>();
builder.Services.AddTransient<DashboardService>();
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddScoped<ProfileService>();
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
    })
    .AddEntityFrameworkStores<SqlServerDbContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Citizen/Login"; // This is the path that will be used for unauthorized redirects
        options.AccessDeniedPath = "/Citizen/HandleError?statusCode=403";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("User", policy => policy.RequireRole("User"));
    options.AddPolicy("NotUser", policy =>
        policy.RequireAssertion(context =>
            !context.User.IsInRole("User")));
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Citizen/HandleError", "?statusCode={0}");

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Citizen}/{action=Route}");

app.Run();
