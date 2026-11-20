namespace Lab5.Services;

public enum ServerType { Web, Database, Cache, LoadBalancer, FileStorage }

/// <summary>Стан сервера в системі моніторингу.</summary>
public enum ServerStatus { Online, Offline, Maintenance }

/// <summary>Сервер, що перебуває під моніторингом.</summary>
public class MonitoredServer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public ServerType Type { get; set; }
    public string Host { get; set; } = string.Empty;            // IP або hostname
    public ServerStatus Status { get; set; } = ServerStatus.Online;
    public decimal? CpuUsage { get; set; }                       // 0..100 %
    public decimal? MemoryUsage { get; set; }                    // 0..100 %
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
