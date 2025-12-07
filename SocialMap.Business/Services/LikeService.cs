using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.Business.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IPostRepository _postRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public LikeService(ILikeRepository likeRepository, IPostRepository postRepository, IUserRepository userRepository, INotificationService notificationService)
    {
        _likeRepository = likeRepository;
        _postRepository = postRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<Like?> GetLikeByIdAsync(Guid id)
    {
        return await _likeRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Like>> GetLikesByPostIdAsync(Guid postId)
    {
        return await _likeRepository.GetByPostIdAsync(postId);
    }

    public async Task<IEnumerable<Like>> GetLikesByUserIdAsync(Guid userId)
    {
        return await _likeRepository.GetByUserIdAsync(userId);
    }

    public async Task<Like> AddLikeAsync(Guid postId, Guid userId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            throw new InvalidOperationException($"Post with ID '{postId}' not found.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID '{userId}' not found.");

        var existingLike = await _likeRepository.GetByPostAndUserAsync(postId, userId);
        if (existingLike != null)
            throw new InvalidOperationException("Post already liked by this user.");

        var like = new Like
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var addedLike = await _likeRepository.AddAsync(like);
        
        // Post'un likes count'unu artır
        post.LikesCount++;
        await _postRepository.UpdateAsync(post);

        // Bildirim oluştur (post sahibine)
        if (post.UserId != userId)
        {
            await _notificationService.CreateNotificationAsync(
                post.UserId,
                "like",
                $"{user.Username} gönderinizi beğendi",
                relatedPostId: postId,
                relatedUserId: userId
            );
        }

        return addedLike;
    }

    public async Task RemoveLikeAsync(Guid postId, Guid userId)
    {
        var like = await _likeRepository.GetByPostAndUserAsync(postId, userId);
        if (like == null)
            throw new InvalidOperationException("Like not found.");

        await _likeRepository.DeleteAsync(like);
        
        // Post'un likes count'unu azalt
        var post = await _postRepository.GetByIdAsync(postId);
        if (post != null && post.LikesCount > 0)
        {
            post.LikesCount--;
            await _postRepository.UpdateAsync(post);
        }
    }

    public async Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId)
    {
        return await _likeRepository.IsLikedAsync(postId, userId);
    }

    public async Task<int> GetLikeCountByPostIdAsync(Guid postId)
    {
        var likes = await _likeRepository.GetByPostIdAsync(postId);
        return likes.Count();
    }
}

