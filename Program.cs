using GovConnect.Chat;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICitizenRepository, CitizenRepository>();
builder.Services.AddScoped<ICitizenService, CitizenService>();
builder.Services.AddScoped<IOfficerService, OfficerService>();
builder.Services.AddScoped<IOfficerRepository, OfficerRepository>();
builder.Services.AddScoped<ISchemeRepository,SchemeRepository>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IGrievanceRepository, GrievanceRepository>();
builder.Services.AddScoped<ISchemeService, SchemeService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IGrievanceService, GrievanceService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddTransient<DashboardService>();
builder.Services.AddSingleton<EmailSender>();
builder.Services.AddSignalR();
builder.Services.AddHttpClient<PincodeService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(30);
});
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
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    });
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Citizen/Login";
    options.AccessDeniedPath = "/Citizen/AccessDenied";
});
var razorpayKeyId = builder.Configuration["Razorpay:KeyId"];
var razorpaySecretKey = builder.Configuration["Razorpay:SecretKey"];

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy => policy.RequireRole("User"));
    options.AddPolicy("OfficerPolicy", policy => policy.RequireRole("Officer"));
});
builder.Services.AddHttpClient();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStaticFiles();

app.UseSession();

app.UseStatusCodePagesWithReExecute("/Citizen/HandleError", "?statusCode={0}");

app.UseHttpsRedirection();

app.MapHub<ChatHub>("/chathub");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Citizen}/{action=Route}");
app.MapControllerRoute(
	name: "DetailsRoute",
	pattern: "Details/{Id}",
	defaults: new { controller = "Product", action = "Details" });
app.Run();
