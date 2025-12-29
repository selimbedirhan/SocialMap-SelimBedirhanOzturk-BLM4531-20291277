using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SocialMap.Core.Interfaces;

namespace SocialMap.Repository.Data;

/// <summary>
/// Unit of Work implementation - Transaction yönetimi
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;
    
    private IUserRepository? _users;
    private IPostRepository? _posts;
    private ICommentRepository? _comments;
    private ILikeRepository? _likes;
    private IFollowRepository? _follows;
    private IPlaceRepository? _places;
    private INotificationRepository? _notifications;

    public UnitOfWork(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ILikeRepository likeRepository,
        IFollowRepository followRepository,
        IPlaceRepository placeRepository,
        INotificationRepository notificationRepository)
    {
        _context = context;
        _users = userRepository;
        _posts = postRepository;
        _comments = commentRepository;
        _likes = likeRepository;
        _follows = followRepository;
        _places = placeRepository;
        _notifications = notificationRepository;
    }

    public IUserRepository Users => _users!;
    public IPostRepository Posts => _posts!;
    public ICommentRepository Comments => _comments!;
    public ILikeRepository Likes => _likes!;
    public IFollowRepository Follows => _follows!;
    public IPlaceRepository Places => _places!;
    public INotificationRepository Notifications => _notifications!;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
