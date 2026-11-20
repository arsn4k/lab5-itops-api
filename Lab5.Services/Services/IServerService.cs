namespace Lab5.Services;

public interface IServerService
{
    Task<IEnumerable<ServerDto>> SearchServersAsync(string name);
    Task<ServerDto?> GetServerDetailsAsync(Guid id);
    Task<ServerDto> CreateServerAsync(CreateServerRequest request);
    Task<ServerDto?> UpdateServerAsync(Guid id, UpdateServerRequest request);
    Task<bool> SetServerStatusAsync(Guid id, string status);
    Task<bool> DeleteServerAsync(Guid id);
}
