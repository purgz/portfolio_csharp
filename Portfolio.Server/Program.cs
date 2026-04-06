using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Server.Data;
using System.Text;
using Portfolio.Server.Models;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Serves the Blazor WASM client
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
  options.Password.RequiredLength = 6;
  options.Password.RequireDigit = true;

  //options.Lockout.MaxFailedAccessAttempts = 5;
  //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();

app.UseBlazorFrameworkFiles();  // serves Blazor WASM
app.UseStaticFiles();           // serves wwwroot

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();           // API routes e.g. /api/auth/login
app.MapRazorPages();
app.MapFallbackToFile("index.html"); // Blazor client-side routing fallback

// Migrate and seed on startup
// Causes crash to the web server if database is not available
try
{
    using (var scope = app.Services.CreateScope())
    {
        
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        /*
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var username = "";
        var password = "";

        var existingAdmin = await userManager.FindByNameAsync(username);
        existingAdmin = null;
        if (existingAdmin == null)
        {
          var user = new ApplicationUser
          {
            UserName = username!,
            Activated = false
          };
          var result =await userManager.CreateAsync(user, password);
          if (!result.Succeeded)
          {
            Console.WriteLine("Failed to create admin user:");
            foreach (var error in result.Errors)
            {
              Console.WriteLine($"- {error.Description}");
            }
          }
        }
        */

        if (!db.LeagueResults.Any())
          {
              db.LeagueResults.AddRange(
                  new Portfolio.Server.Models.LeagueResult { Year = 2025, Semester = 1, PlayerName = "NEW USER", Score = 35.5f }
              );
              db.SaveChanges();
          }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Database migration failed: {ex.Message}");
    Console.WriteLine("Database may not be available - app startup will continue, but API calls will fail until database is online");
}


app.Run();