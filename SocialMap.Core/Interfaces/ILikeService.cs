using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface ILikeService
{
    Task<Like?> GetLikeByIdAsync(Guid id);
    Task<IEnumerable<Like>> GetLikesByPostIdAsync(Guid postId);
    Task<IEnumerable<Like>> GetLikesByUserIdAsync(Guid userId);
    Task<Like> AddLikeAsync(Guid postId, Guid userId);
    Task RemoveLikeAsync(Guid postId, Guid userId);
    Task<bool> IsPostLikedByUserAsync(Guid postId, Guid userId);
    Task<int> GetLikeCountByPostIdAsync(Guid postId);
}

