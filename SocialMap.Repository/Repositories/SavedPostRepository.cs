using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class SavedPostRepository : Repository<SavedPost>, ISavedPostRepository
{
    public SavedPostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SavedPost?> GetByUserAndPostAsync(Guid userId, Guid postId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);
    }

    public async Task<IEnumerable<SavedPost>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(sp => sp.Post)
                .ThenInclude(p => p!.User)
            .Include(sp => sp.Post)
                .ThenInclude(p => p!.Place)
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.SavedAt)
            .ToListAsync();
    }

    public async Task<bool> IsSavedAsync(Guid userId, Guid postId)
    {
        return await _dbSet.AnyAsync(sp => sp.UserId == userId && sp.PostId == postId);
    }

    public async Task<int> GetSaveCountAsync(Guid postId)
    {
        return await _dbSet.CountAsync(sp => sp.PostId == postId);
    }
}
