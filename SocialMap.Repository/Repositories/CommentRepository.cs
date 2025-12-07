using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<Comment?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Post)
            .Include(c => c.ParentComment)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Comment>> GetByPostIdAsync(Guid postId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(c => c.Post)
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetRepliesByParentIdAsync(Guid parentCommentId)
    {
        return await _dbSet
            .Include(c => c.User)
            .Where(c => c.ParentCommentId == parentCommentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }
}

