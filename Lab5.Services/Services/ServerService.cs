using Microsoft.Extensions.Logging;

namespace Lab5.Services;

public class ServerService : IServerService
{
    private readonly IServerRepository _repo;
    private readonly ILogger<ServerService> _logger;

    public ServerService(IServerRepository repo, ILogger<ServerService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    private static ServerDto ToDto(MonitoredServer s) =>
        new(s.Id, s.Name, s.Type, s.Host, s.Status, s.CpuUsage, s.MemoryUsage, s.CreatedAtUtc);

    public async Task<IEnumerable<ServerDto>> SearchServersAsync(string name)
    {
        var all = await _repo.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(name))
            all = all.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        return all.Select(ToDto).ToList();
    }

    public async Task<ServerDto?> GetServerDetailsAsync(Guid id)
    {
        var s = await _repo.GetByIdAsync(id);
        return s is null ? null : ToDto(s);
    }

    public async Task<ServerDto> CreateServerAsync(CreateServerRequest r)
    {
        var server = new MonitoredServer
        {
            Name = r.Name.Trim(),
            Type = r.Type,
            Host = r.Host.Trim(),
            CpuUsage = r.CpuUsage,
            MemoryUsage = r.MemoryUsage
        };
        await _repo.AddAsync(server);
        _logger.LogInformation("Створено сервер {ServerId} ({ServerName})", server.Id, server.Name);
        return ToDto(server);
    }

    public async Task<ServerDto?> UpdateServerAsync(Guid id, UpdateServerRequest r)
    {
        var s = await _repo.GetByIdAsync(id);
        if (s is null) return null;

        s.Name = r.Name.Trim();
        s.Type = r.Type;
        s.Host = r.Host.Trim();
        s.CpuUsage = r.CpuUsage;
        s.MemoryUsage = r.MemoryUsage;
        await _repo.UpdateAsync(s);
        _logger.LogInformation("Оновлено сервер {ServerId}", s.Id);
        return ToDto(s);
    }

    public async Task<bool> SetServerStatusAsync(Guid id, string status)
    {
        // Валідатор уже перевірив значення; ця перевірка захищає сервіс, якщо його викличуть напряму.
        if (!Enum.TryParse<ServerStatus>(status, ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
            throw new ArgumentException($"Недопустимий стан сервера: {status}", nameof(status));

        var s = await _repo.GetByIdAsync(id);
        if (s is null) return false;

        s.Status = parsed;
        await _repo.UpdateAsync(s);
        _logger.LogInformation("Сервер {ServerId} переведено у стан {Status}", s.Id, parsed);
        return true;
    }

    public async Task<bool> DeleteServerAsync(Guid id)
    {
        var deleted = await _repo.DeleteAsync(id);
        if (deleted) _logger.LogInformation("Видалено сервер {ServerId}", id);
        return deleted;
    }
}
