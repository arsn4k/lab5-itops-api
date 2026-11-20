namespace Lab5.Services;

public sealed record ServerDto(
    Guid Id,
    string Name,
    ServerType Type,
    string Host,
    ServerStatus Status,
    decimal? CpuUsage,
    decimal? MemoryUsage,
    DateTime CreatedAtUtc);
