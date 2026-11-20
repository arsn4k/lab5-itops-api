namespace Lab5.Services;

public interface IServerRepository
{
    Task<IEnumerable<MonitoredServer>> GetAllAsync();
    Task<MonitoredServer?> GetByIdAsync(Guid id);
    Task AddAsync(MonitoredServer server);
    Task UpdateAsync(MonitoredServer server);
    Task<bool> DeleteAsync(Guid id);
}
