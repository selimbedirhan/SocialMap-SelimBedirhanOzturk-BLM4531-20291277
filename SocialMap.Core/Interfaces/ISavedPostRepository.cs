using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface ISavedPostRepository : IRepository<SavedPost>
{
    Task<SavedPost?> GetByUserAndPostAsync(Guid userId, Guid postId);
    Task<IEnumerable<SavedPost>> GetByUserIdAsync(Guid userId);
    Task<bool> IsSavedAsync(Guid userId, Guid postId);
    Task<int> GetSaveCountAsync(Guid postId);
}
