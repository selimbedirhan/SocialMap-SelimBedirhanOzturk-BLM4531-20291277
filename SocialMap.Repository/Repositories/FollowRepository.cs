using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class FollowRepository : Repository<Follow>, IFollowRepository
{
    public FollowRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Follow?> GetFollowAsync(Guid followerId, Guid followingId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<IEnumerable<Follow>> GetFollowersAsync(Guid userId)
    {
        return await _dbSet
            .Include(f => f.Follower)
            .Where(f => f.FollowingId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Follow>> GetFollowingAsync(Guid userId)
    {
        return await _dbSet
            .Include(f => f.Following)
            .Where(f => f.FollowerId == userId)
            .ToListAsync();
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId)
    {
        return await _dbSet
            .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<int> GetFollowerCountAsync(Guid userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowingId == userId);
    }

    public async Task<int> GetFollowingCountAsync(Guid userId)
    {
        return await _dbSet
            .CountAsync(f => f.FollowerId == userId);
    }
}

