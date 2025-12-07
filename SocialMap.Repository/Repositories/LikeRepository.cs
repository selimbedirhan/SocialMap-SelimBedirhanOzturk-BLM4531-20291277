using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class LikeRepository : Repository<Like>, ILikeRepository
{
    public LikeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Like?> GetByPostAndUserAsync(Guid postId, Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
    }

    public async Task<IEnumerable<Like>> GetByPostIdAsync(Guid postId)
    {
        return await _dbSet
            .Where(l => l.PostId == postId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Like>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsLikedAsync(Guid postId, Guid userId)
    {
        return await _dbSet
            .AnyAsync(l => l.PostId == postId && l.UserId == userId);
    }
}

