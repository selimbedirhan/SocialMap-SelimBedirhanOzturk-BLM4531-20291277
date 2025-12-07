using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface IFollowRepository : IRepository<Follow>
{
    Task<Follow?> GetFollowAsync(Guid followerId, Guid followingId);
    Task<IEnumerable<Follow>> GetFollowersAsync(Guid userId);
    Task<IEnumerable<Follow>> GetFollowingAsync(Guid userId);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followingId);
    Task<int> GetFollowerCountAsync(Guid userId);
    Task<int> GetFollowingCountAsync(Guid userId);
}

