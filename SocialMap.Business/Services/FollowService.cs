using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;

namespace SocialMap.Business.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public FollowService(IFollowRepository followRepository, IUserRepository userRepository, INotificationService notificationService)
    {
        _followRepository = followRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<Follow> FollowUserAsync(Guid followerId, Guid followingId)
    {
        if (followerId == followingId)
            throw new InvalidOperationException("Kendinizi takip edemezsiniz.");

        var follower = await _userRepository.GetByIdAsync(followerId);
        if (follower == null)
            throw new InvalidOperationException($"Follower user not found.");

        var following = await _userRepository.GetByIdAsync(followingId);
        if (following == null)
            throw new InvalidOperationException($"Following user not found.");

        var existingFollow = await _followRepository.GetFollowAsync(followerId, followingId);
        if (existingFollow != null)
            throw new InvalidOperationException("Zaten bu kullanıcıyı takip ediyorsunuz.");

        var follow = new Follow
        {
            Id = Guid.NewGuid(),
            FollowerId = followerId,
            FollowingId = followingId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _followRepository.AddAsync(follow);

        // Bildirim oluştur (SignalR gönderme WebAPI'de yapılacak)
        await _notificationService.CreateNotificationAsync(
            followingId,
            "follow",
            $"{follower.Username} sizi takip etmeye başladı",
            relatedUserId: followerId
        );

        return result;
    }

    public async Task UnfollowUserAsync(Guid followerId, Guid followingId)
    {
        var follow = await _followRepository.GetFollowAsync(followerId, followingId);
        if (follow == null)
            throw new InvalidOperationException("Bu kullanıcıyı takip etmiyorsunuz.");

        await _followRepository.DeleteAsync(follow);
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId)
    {
        return await _followRepository.IsFollowingAsync(followerId, followingId);
    }

    public async Task<IEnumerable<User>> GetFollowersAsync(Guid userId)
    {
        var follows = await _followRepository.GetFollowersAsync(userId);
        return follows.Select(f => f.Follower);
    }

    public async Task<IEnumerable<User>> GetFollowingAsync(Guid userId)
    {
        var follows = await _followRepository.GetFollowingAsync(userId);
        return follows.Select(f => f.Following);
    }

    public async Task<int> GetFollowerCountAsync(Guid userId)
    {
        return await _followRepository.GetFollowerCountAsync(userId);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        return await _followRepository.GetFollowingCountAsync(userId);
    }
}

