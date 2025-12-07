using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId);
    Task<IEnumerable<Comment>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Comment>> GetRepliesByParentIdAsync(Guid parentCommentId);
}

