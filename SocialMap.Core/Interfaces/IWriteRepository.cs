namespace SocialMap.Core.Interfaces;

/// <summary>
/// Write-only repository interface (Interface Segregation Principle)
/// Sadece yazma operasyonları için kullanılır
/// </summary>
public interface IWriteRepository<T> where T : class
{
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task UpdateRangeAsync(IEnumerable<T> entities);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
}
