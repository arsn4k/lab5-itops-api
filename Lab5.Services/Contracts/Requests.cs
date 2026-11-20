namespace Lab5.Services;

// Правила валідації винесено в окремі класи-валідатори (Lab5.API/Validators).

public sealed class CreateServerRequest
{
    public string Name { get; set; } = string.Empty;
    public ServerType Type { get; set; }
    public string Host { get; set; } = string.Empty;
    public decimal? CpuUsage { get; set; }
    public decimal? MemoryUsage { get; set; }
}

public sealed class UpdateServerRequest
{
    public string Name { get; set; } = string.Empty;
    public ServerType Type { get; set; }
    public string Host { get; set; } = string.Empty;
    public decimal? CpuUsage { get; set; }
    public decimal? MemoryUsage { get; set; }
}

public sealed class SetServerStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
