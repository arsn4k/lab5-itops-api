using Lab5.Services;
using Microsoft.EntityFrameworkCore;

namespace Lab5.Infrastructure.Data;

public sealed class Lab5DbContext : DbContext
{
    public Lab5DbContext(DbContextOptions<Lab5DbContext> options) : base(options)
    {
    }

    public DbSet<MonitoredServer> Servers => Set<MonitoredServer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MonitoredServer>(e =>
        {
            e.ToTable("Servers");
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).IsRequired().HasMaxLength(100);
            e.Property(s => s.Host).IsRequired().HasMaxLength(255);
            // enum зберігаються рядками ("Database", "Online"), а не числами
            e.Property(s => s.Type).HasConversion<string>().HasMaxLength(30);
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(s => s.CpuUsage).HasPrecision(5, 2);
            e.Property(s => s.MemoryUsage).HasPrecision(5, 2);
            e.HasIndex(s => s.Name);
        });
    }
}
