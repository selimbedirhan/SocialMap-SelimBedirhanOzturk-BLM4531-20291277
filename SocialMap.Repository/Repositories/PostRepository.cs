using Microsoft.EntityFrameworkCore;
using SocialMap.Core.Entities;
using SocialMap.Core.Interfaces;
using SocialMap.Repository.Data;

namespace SocialMap.Repository.Repositories;

public class PostRepository : Repository<Post>, IPostRepository
{
    public PostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public new async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
                .ThenInclude(l => l.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public new async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Place)
            .Include(p => p.Comments)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetByPlaceIdAsync(Guid placeId)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .Where(p => p.PlaceId == placeId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetRecentPostsAsync(int count)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
            .Include(p => p.Likes)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsWithinBoundsAsync(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.Latitude.HasValue && p.Longitude.HasValue &&
                       p.Latitude.Value >= minLatitude && p.Latitude.Value <= maxLatitude &&
                       p.Longitude.Value >= minLongitude && p.Longitude.Value <= maxLongitude)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Post>> GetPostsByGeohashPrefixAsync(string geohashPrefix)
    {
        return await _dbSet
            .Include(p => p.User)
            .Include(p => p.Place)
            .Include(p => p.Comments)
            .Include(p => p.Likes)
            .Where(p => p.Geohash != null && p.Geohash.StartsWith(geohashPrefix))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}

