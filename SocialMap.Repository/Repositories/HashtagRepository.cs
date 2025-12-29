using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class HashtagRepository : Repository<Hashtag>, IHashtagRepository
{
    private readonly ApplicationDbContext _context;

    public HashtagRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Hashtag?> GetByNameAsync(string name)
    {
        return await _dbSet
            .FirstOrDefaultAsync(h => h.Name == name.ToLower());
    }

    public async Task<IEnumerable<Hashtag>> GetTrendingAsync(int count = 10)
    {
        return await _dbSet
            .OrderByDescending(h => h.UsageCount)
            .ThenByDescending(h => h.LastUsedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Hashtag>> SearchAsync(string query, int limit = 20)
    {
        var lowerQuery = query.ToLower().TrimStart('#');
        return await _dbSet
            .Where(h => h.Name.Contains(lowerQuery))
            .OrderByDescending(h => h.UsageCount)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByHashtagAsync(string hashtagName, int page = 1, int pageSize = 20)
    {
        var hashtag = await GetByNameAsync(hashtagName.ToLower().TrimStart('#'));
        if (hashtag == null)
            return new List<Post>();

        return await _context.PostHashtags
            .Where(ph => ph.HashtagId == hashtag.Id)
            .Include(ph => ph.Post)
                .ThenInclude(p => p!.User)
            .Include(ph => ph.Post)
                .ThenInclude(p => p!.Place)
            .OrderByDescending(ph => ph.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ph => ph.Post!)
            .ToListAsync();
    }
}
