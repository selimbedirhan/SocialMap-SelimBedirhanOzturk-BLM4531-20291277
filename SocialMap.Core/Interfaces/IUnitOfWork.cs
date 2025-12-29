namespace SocialMap.Core.Interfaces;

/// <summary>
/// Unit of Work pattern - Transaction yönetimi için
/// Birden fazla repository'de yapılan değişiklikleri tek bir transaction'da commit eder
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPostRepository Posts { get; }
    ICommentRepository Comments { get; }
    ILikeRepository Likes { get; }
    IFollowRepository Follows { get; }
    IPlaceRepository Places { get; }
    INotificationRepository Notifications { get; }
    
    /// <summary>
    /// Tüm değişiklikleri veritabanına commit eder
    /// </summary>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Transaction başlatır
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Transaction'ı commit eder
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Transaction'ı geri alır
    /// </summary>
    Task RollbackTransactionAsync();
}
