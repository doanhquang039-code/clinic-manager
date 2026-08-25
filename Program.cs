using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); // For API endpoints

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = "ExternalCookie";
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
})
.AddCookie("ExternalCookie", options =>
{
    options.Cookie.Name = "ExternalCookie";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10); // chỉ tồn tại tạm thời
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? (builder.Configuration["Authentication:Google:ClientId1"] + builder.Configuration["Authentication:Google:ClientId2"]);
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? (builder.Configuration["Authentication:Google:ClientSecret1"] + builder.Configuration["Authentication:Google:ClientSecret2"]);
    googleOptions.Events = new OAuthEvents
    {
        OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
            return Task.CompletedTask;
        }
    };
})
.AddGitHub(githubOptions =>
{
    githubOptions.ClientId = builder.Configuration["Authentication:GitHub:ClientId"] ?? (builder.Configuration["Authentication:GitHub:ClientId1"] + builder.Configuration["Authentication:GitHub:ClientId2"]);
    githubOptions.ClientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"] ?? (builder.Configuration["Authentication:GitHub:ClientSecret1"] + builder.Configuration["Authentication:GitHub:ClientSecret2"]);
})
.AddMicrosoftAccount(microsoftOptions =>
{
    microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? (builder.Configuration["Authentication:Microsoft:ClientId1"] + builder.Configuration["Authentication:Microsoft:ClientId2"]);
    microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? (builder.Configuration["Authentication:Microsoft:ClientSecret1"] + builder.Configuration["Authentication:Microsoft:ClientSecret2"]);
    microsoftOptions.Events = new OAuthEvents
    {
        OnRedirectToAuthorizationEndpoint = context =>
        {
            context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped(typeof(MyMvcApp.Repositories.IRepository<>), typeof(MyMvcApp.Repositories.Repository<>));
builder.Services.AddScoped<MyMvcApp.Services.IImageService, MyMvcApp.Services.CloudinaryService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MyMvcApp.Models.ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Run migrations on startup (important for MonsterASP since remote connection is blocked)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<MyMvcApp.Models.ApplicationDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Tạm tắt để chạy được trên HTTP thường
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API routes (Postman)
app.MapControllers();

app.Run();
