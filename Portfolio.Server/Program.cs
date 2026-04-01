using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Server.Data;
using System.Text;

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
}

app.Run();