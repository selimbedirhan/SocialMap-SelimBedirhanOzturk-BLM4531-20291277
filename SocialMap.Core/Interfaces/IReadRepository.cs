using System.Linq.Expressions;

namespace SocialMap.Core.Interfaces;

/// <summary>
/// Read-only repository interface (Interface Segregation Principle)
/// Sadece okuma operasyonları için kullanılır
/// </summary>
public interface IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}
