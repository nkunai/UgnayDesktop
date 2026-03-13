using Microsoft.EntityFrameworkCore;
using UgnayDesktop.Models;

namespace UgnayDesktop.Data;

public class AppDbContext : DbContext
{
    private static readonly string DatabasePath = Path.Combine(AppContext.BaseDirectory, "ugnay.db");

    public DbSet<User> Users { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<OtpChallenge> OtpChallenges { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DatabasePath}");
}
