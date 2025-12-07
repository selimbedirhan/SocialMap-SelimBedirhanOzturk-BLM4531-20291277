using SocialMap.Core.Entities;

namespace SocialMap.Core.Interfaces;

public interface ICommentService
{
    Task<Comment?> GetCommentByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(Guid postId);
    Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(Guid userId);
    Task<IEnumerable<Comment>> GetRepliesByCommentIdAsync(Guid parentCommentId);
    Task<Comment> CreateCommentAsync(Guid postId, Guid userId, string text, Guid? parentCommentId = null);
    Task UpdateCommentAsync(Comment comment);
    Task DeleteCommentAsync(Guid id);
}

