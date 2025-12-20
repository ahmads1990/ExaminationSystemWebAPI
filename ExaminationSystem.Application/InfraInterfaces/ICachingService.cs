namespace ExaminationSystem.Application.InfraInterfaces;

public interface ICachingService
{
    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    public Task AddAsync(string key, string value, TimeSpan? timeToLive, CancellationToken cancellationToken = default);
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
