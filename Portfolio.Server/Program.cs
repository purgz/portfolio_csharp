using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Server.Data;
using System.Text;
using Portfolio.Server.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Serves the Blazor WASM client
builder.Services.AddRazorPages();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

        if (!db.AdminUsers.Any())
        {
            db.AdminUsers.Add(new Portfolio.Server.Models.AdminUser
            {
                Username = builder.Configuration["Admin:Username"]!,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                    builder.Configuration["Admin:SeedPassword"]!)
            });
            db.SaveChanges();
        }

        if (!db.LeagueResults.Any())
          {
              db.LeagueResults.AddRange(
                  // 2023
                  new Portfolio.Server.Models.LeagueResult { Year = 2023, Semester = 1, PlayerName = "Alice", Score = 15 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2023, Semester = 1, PlayerName = "Bob", Score = 12 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2023, Semester = 1, PlayerName = "Charlie", Score = 10 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2023, Semester = 2, PlayerName = "Alice", Score = 13 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2023, Semester = 2, PlayerName = "Bob", Score = 17 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2023, Semester = 2, PlayerName = "Charlie", Score = 9 },
                  // 2024
                  new Portfolio.Server.Models.LeagueResult { Year = 2024, Semester = 1, PlayerName = "Alice", Score = 18 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2024, Semester = 1, PlayerName = "Bob", Score = 14 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2024, Semester = 1, PlayerName = "Charlie", Score = 11 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2024, Semester = 2, PlayerName = "Alice", Score = 16 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2024, Semester = 2, PlayerName = "Bob", Score = 19 },
                  new Portfolio.Server.Models.LeagueResult { Year = 2024, Semester = 2, PlayerName = "Charlie", Score = 14 }
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