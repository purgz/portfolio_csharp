using Microsoft.EntityFrameworkCore;
using Portfolio.Server.Models;

namespace Portfolio.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<AdminUser> AdminUsers { get; set; }
    public DbSet<LeagueResult> LeagueResults { get; set; }
}