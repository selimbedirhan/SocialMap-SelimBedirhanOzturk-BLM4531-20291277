using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface ILikeRepository : IRepository<Like>
{
    Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId);
    Task<IEnumerable<Like>> GetByPostIdAsync(Guid postId);
    Task<IEnumerable<Like>> GetByUserIdAsync(Guid userId);
    Task<bool> IsLikedAsync(Guid postId, Guid userId);
}

