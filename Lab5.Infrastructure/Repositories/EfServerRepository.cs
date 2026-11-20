using Lab5.Infrastructure.Data;
using Lab5.Services;
using Microsoft.EntityFrameworkCore;

namespace Lab5.Infrastructure.Repositories;

public class EfServerRepository : IServerRepository
{
    private readonly Lab5DbContext _db;

    public EfServerRepository(Lab5DbContext db) => _db = db;

    public async Task<IEnumerable<MonitoredServer>> GetAllAsync() =>
        await _db.Servers.AsNoTracking().OrderBy(s => s.Name).ToListAsync();

    public async Task<MonitoredServer?> GetByIdAsync(Guid id) =>
        await _db.Servers.FindAsync(id);

    public async Task AddAsync(MonitoredServer server)
    {
        _db.Servers.Add(server);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(MonitoredServer server)
    {
        _db.Servers.Update(server);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var server = await _db.Servers.FindAsync(id);
        if (server is null) return false;

        _db.Servers.Remove(server);
        await _db.SaveChangesAsync();
        return true;
    }
}
