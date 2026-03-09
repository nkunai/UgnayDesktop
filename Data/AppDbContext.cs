using Microsoft.EntityFrameworkCore;
using UgnayDesktop.Models;

namespace UgnayDesktop.Data;

public class AppDbContext : DbContext
{
    private static readonly string DatabasePath = Path.Combine(AppContext.BaseDirectory, "ugnay.db");

    public DbSet<User> Users { get; set; }
    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<AlertOutboxMessage> AlertOutboxMessages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DatabasePath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.Username).HasMaxLength(64);
            entity.Property(x => x.PasswordHash).HasMaxLength(256);
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.Role).HasMaxLength(16);
            entity.Property(x => x.FullName).HasMaxLength(128);
            entity.Property(x => x.TeacherPhoneNumber).HasMaxLength(32);
            entity.Property(x => x.DeviceId).HasMaxLength(128);
            entity.Property(x => x.Sex).HasMaxLength(16);
            entity.Property(x => x.ThemePreference).HasMaxLength(16).HasDefaultValue("Light");
            entity.Property(x => x.MustChangePassword).HasDefaultValue(false);
            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.Property(x => x.DeviceId).HasMaxLength(128);
            entity.Property(x => x.HandGesture).HasMaxLength(64);
            entity.HasIndex(x => new { x.DeviceId, x.ReceivedAtUtc, x.RawJson }).IsUnique();
            entity.HasIndex(x => x.ReceivedAtUtc);
        });

        modelBuilder.Entity<AlertOutboxMessage>(entity =>
        {
            entity.Property(x => x.Source).HasMaxLength(64);
            entity.Property(x => x.DeviceId).HasMaxLength(128);
            entity.Property(x => x.Severity).HasMaxLength(16);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
            entity.Property(x => x.RecipientPhoneNumber).HasMaxLength(32);
            entity.Property(x => x.ProviderMessageId).HasMaxLength(128);
            entity.HasIndex(x => x.DedupKey).IsUnique();
            entity.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        });

        base.OnModelCreating(modelBuilder);
    }
}


