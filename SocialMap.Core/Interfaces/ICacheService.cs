namespace SocialMap.Core.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan duration);
    Task RemoveAsync(string key);
}
